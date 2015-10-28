var g_AppPrefix = '/';

var g_lastSeenMessageId = -1;
var g_DiagnosticMessagesLoaderInterval = 500;

var g_TargetEnvironmentCookieName = 'target-environment-name';
var g_TargetEnvironmentCookieExpirationInDays = 365;

var g_ProjectList = [];
var g_ProjectNames = [];
var g_EnvironmentList = [];

var g_userCanDeploy = false;
var g_initialSelection = null;

var KICKASSVERSION = '2.0';

function setAppPrefix(appPrefix) {
  g_AppPrefix = appPrefix;
}

var APP_TYPES = {
  NotSet: 0,
  Db: 1,
  NtService: 2,
  SchedulerApp: 3,
  TerminalApp: 4,
  WebApp: 5,
  WebService: 6,
  UberDeployerAgent: 7
};

function Project(name, type, allowedEnvironmentNames) {
  var self = this;

  self.name = '';
  self.type = '';
  self.allowedEnvironmentNames = [];

  self.update = function (projectName, projectType, allowedEnvironmentNamesForProject) {
    self.name = projectName;
    self.type = projectType;
    self.allowedEnvironmentNames = allowedEnvironmentNamesForProject;
  };

  self.update(name, type, allowedEnvironmentNames);
}

function Environment(name, isDeployable) {
  var self = this;

  self.name = name;
  self.isDeployable = isDeployable;
}

function initializeDeploymentPage(initData) {
  g_userCanDeploy = initData.userCanDeploy;
  g_initialSelection = initData.initialSelection;

  var collectCredentialsDialog = new CollectCredentialsDialog();
  var collectScriptsToRunDialog = new CollectScriptsToRunDialog(false);
  var collectProjectDependenciesDialog = new CollectProjectDependenciesDialog();

  setupSignalR(collectCredentialsDialog, collectScriptsToRunDialog, collectProjectDependenciesDialog);

  $.ajaxSetup({
    'error': function (xhr) {
      domHelper.showError(xhr);
    }
  });

  $('#btn-deploy').click(function () {
    deploy();
  });

  $('#btn-simulate').click(function () {
    simulate();
  });

  $('#btn-create-package').click(function () {
    promptPackageDirPath();
  });

  $('#btn-package-ok').click(function () {
    var packageDir = $('#txt-package-dir')[0].value;

    if ($.trim(packageDir) === '') {
      toastr.error('You have to enter package dir path.');
      return;
    }

    $('#package-dir-modal').modal('hide');

    createPackage(packageDir);
  });

  $('#chb-deployable-projects').change(function () {
    loadProjectsForCurrentEnvironment();
    disableDeployButtonsForCurrentEnvironment();
  });

  domHelper.getProjectsElement().change(function () {
    var projectName = getSelectedProjectName();

    if (!projectName) {
      return;
    }

    clearProjectConfigurationBuilds();
    loadProjectConfigurations(projectName);
    loadWebMachinesList();
    disableDeployButtonsForCurrentEnvironment();
  });

  domHelper.getProjectConfigsElement().change(function () {
    var projectName = getSelectedProjectName();
    var projectConfigurationName = getSelectedProjectConfigurationName();

    if (!projectName || !projectConfigurationName) {
      return;
    }

    clearProjectConfigurationBuilds();

    loadProjectConfigurationBuilds(
      projectName,
      projectConfigurationName,
      function () {
        var valueToSelect;

        if (g_initialSelection && g_initialSelection.projectConfigurationBuildId) {
          valueToSelect = g_initialSelection.projectConfigurationBuildId;
        }
        else {
          valueToSelect = $('#lst-project-config-builds option').eq(0).val();
        }

        g_initialSelection = null;

        var projectConfigBuildsElement = $('#lst-project-config-builds');

        projectConfigBuildsElement.val(valueToSelect);

        if (projectConfigBuildsElement.val() === null) {
          toastr.error('Project configuration has no successful builds.');
          return;
        }

        projectConfigBuildsElement.trigger('change');
      });
  });

  loadEnvironments(function () {
    if (g_initialSelection && g_initialSelection.targetEnvironmentName) {
      selectEnvironment(g_initialSelection.targetEnvironmentName);
    }
    else {
      restoreRememberedTargetEnvironmentName();
    }
  });

  domHelper.getEnvironmentsElement().change(function () {
    if (!g_initialSelection || !g_initialSelection.targetEnvironmentName) {
      rememberTargetEnvironmentName();
    }

    loadProjectsForCurrentEnvironment();
    disableDeployButtonsForCurrentEnvironment();
  });

  startDiagnosticMessagesLoader();
}

function getDeploymentInfo() {
  var deploymentInfo = {
    projectName: getSelectedProjectName(),
    projectConfigurationName: getSelectedProjectConfigurationName(),
    projectConfigurationBuildId: getSelectedProjectConfigurationBuildId(),
    targetEnvironmentName: getSelectedTargetEnvironmentName(),
    targetMachines: getSelectedTargetMachines()
  };

  if (!deploymentInfo.projectName || !deploymentInfo.projectConfigurationName
    || !deploymentInfo.projectConfigurationBuildId || !deploymentInfo.targetEnvironmentName) {
    toastr.error('Select project, configuration, build and environment!');
    return null;
  }

  return deploymentInfo;
}

function doDeployOrSimulate(isSimulation) {
  var deploymentInfo = getDeploymentInfo();

  if (g_ProjectList[deploymentInfo.projectName].type == APP_TYPES.WebApp && (!deploymentInfo.targetMachines || deploymentInfo.targetMachines.length == 0)) {
    toastr.error('Select web machine for selected environment!');
    return;
  }

  var action = isSimulation ? 'Api/Simulate' : 'Api/Deploy';

  $.ajax({
    url: g_AppPrefix + action,
    type: "POST",
    data: {
      projectName: deploymentInfo.projectName,
      projectConfigurationName: deploymentInfo.projectConfigurationName,
      projectConfigurationBuildId: deploymentInfo.projectConfigurationBuildId,
      targetEnvironmentName: deploymentInfo.targetEnvironmentName,
      projectType: g_ProjectList[deploymentInfo.projectName].type,
      targetMachines: deploymentInfo.targetMachines
    },
    traditional: true,
    success: function (data) {
      handleApiErrorIfPresent(data);
    }
  });
}

function deploy() {
  doDeployOrSimulate(false);
}

function simulate() {
  doDeployOrSimulate(true);
}

function createPackage(packageDirPath) {
  var deploymentInfo = getDeploymentInfo();

  $.ajax({
    url: g_AppPrefix + "Api/CreatePackage",
    type: "POST",
    data: {
      projectName: deploymentInfo.projectName,
      projectConfigurationName: deploymentInfo.projectConfigurationName,
      projectConfigurationBuildId: deploymentInfo.projectConfigurationBuildId,
      targetEnvironmentName: deploymentInfo.targetEnvironmentName,
      projectType: g_ProjectList[deploymentInfo.projectName].type,
      packageDirPath: packageDirPath
    },
    traditional: true,
    success: function (data) {
      handleApiErrorIfPresent(data);
    }
  });
}

function promptPackageDirPath() {
  var deploymentInfo = getDeploymentInfo();

  $.get(
   g_AppPrefix + "Api/GetDefaultPackageDirPath",
   {
     environmentName: deploymentInfo.targetEnvironmentName,
     projectName: deploymentInfo.projectName
   })
   .done(
     function (data) {
       $('#txt-package-dir')[0].value = data;
       $('#package-dir-modal').modal('show');
     })
   .error(
     function (data) {
       handleApiErrorIfPresent(data);
     });
}

function loadEnvironments(onFinishedCallback) {
  clearEnvironments();

  $.getJSON(
    g_AppPrefix + 'Api/GetEnvironments',
    function (data) {
      clearEnvironments();
      g_EnvironmentList = [];

      $.each(data.environments, function (i, val) {
        g_EnvironmentList[val.Name] = new Environment(val.Name, val.IsDeployable);

        domHelper.getEnvironmentsElement()
          .append(
            $('<option></option>')
              .attr('value', val.Name)
              .text(val.Name));
      });

      if (onFinishedCallback) {
        onFinishedCallback();
      }
    });
}

function loadWebMachinesList() {
  var $lstEnvironments = domHelper.getEnvironmentsElement();
  var $lstMachines = domHelper.getMachinesElement();
  var selectedProject = domHelper.getProjectsElement().val();

  if (g_ProjectList[selectedProject] === undefined) {
    return;
  }

  clearTargetMachines();

  if (g_ProjectList[selectedProject].type != APP_TYPES.WebApp) {
    $lstMachines.attr('disabled', 'disabled');
    return;
  }

  $.getJSON(
    g_AppPrefix + 'Api/GetWebMachineNames',
    { envName: $lstEnvironments.val() },
    function (machines) {
      var newSelectedProject = domHelper.getProjectsElement().val();

      if (!newSelectedProject) {
        return;
      }

      if (g_ProjectList[newSelectedProject].type != APP_TYPES.WebApp) {
        $lstMachines.attr('disabled', 'disabled');
        return;
      }

      clearTargetMachines();

      $lstMachines.removeAttr('disabled');

      $.each(machines, function (i, val) {
        $lstMachines.append(
          $('<option></option>')
            .attr('value', val)
            .attr('selected', 'selected')
            .text(val));
      });
    });
}

function loadProjectsForCurrentEnvironment() {
  var selectedTargetEnvironmentName =
    getSelectedTargetEnvironmentName();

  if (!selectedTargetEnvironmentName) {
    return;
  }

  doLoadProjects(
    selectedTargetEnvironmentName,
    isOnlyDeployable(),
    function () {
      var projectToSelect;

      if (g_initialSelection && g_initialSelection.projectName) {
        projectToSelect = g_initialSelection.projectName;
      }
      else {
        projectToSelect = $('#lst-projects option').eq(0).val();
      }

      var projectsElement = domHelper.getProjectsElement();

      projectsElement.val(projectToSelect);

      if (projectsElement.val() === null) {
        toastr.error('No project named \'' + projectToSelect + '\'.');
        return;
      }

      projectsElement.trigger('change');
    });
}

function doLoadProjects(environmentName, onlyDeployable, onFinishedCallback) {
  clearProjects();
  clearProjectConfigurations();
  clearProjectConfigurationBuilds();

  $.getJSON(g_AppPrefix + 'Api/GetProjects', { environmentName: environmentName, onlyDeployable: onlyDeployable })
    .done(
      function (data) {
        g_ProjectList = [];
        g_ProjectNames = [];
        clearProjects();

        $.each(data.projects, function (i, val) {
          g_ProjectList[val.Name] = new Project(val.Name, val.Type, val.AllowedEnvironmentNames);
          g_ProjectNames.push(val.Name);

          domHelper.getProjectsElement()
            .append(
              $('<option></option>')
                .attr('value', val.Name)
                .text(val.Name));
        });

        if (onFinishedCallback) {
          onFinishedCallback();
        }
      });
}

function disableDeployButtonsForCurrentEnvironment() {
  var selectedEnvironmentName = getSelectedTargetEnvironmentName();
  var projectName = getSelectedProjectName();

  if (!selectedEnvironmentName || !projectName) {
    return;
  }

  var environment = g_EnvironmentList[selectedEnvironmentName];
  var project = g_ProjectList[projectName];

  if (!environment || !project) {
    return;
  }
}

function loadProjectConfigurations(projectName, onFinishedCallback) {
  clearProjectConfigurations();

  $.getJSON(
      g_AppPrefix + 'Api/GetProjectConfigurations?projectName=' + encodeURIComponent(projectName),
      function (data) {
        clearProjectConfigurations();

        var valueToSelect = null;

        $.each(data.projectConfigurations, function (i, val) {
          var $lstProjectConfigs = $('#lst-project-configs');

          var projectConfiguration = val.Name;
          if (val.BranchName) {
            projectConfiguration = val.BranchName + " [" + val.Name + "]";
          }

          var projectConfigurationUpper = projectConfiguration.toUpperCase();

          if (valueToSelect === null && (projectConfigurationUpper === 'TRUNK' || projectConfigurationUpper === 'PRODUCTION' || projectConfigurationUpper === 'DEFAULT' || projectConfigurationUpper === 'MASTER')) {
            valueToSelect = projectConfiguration;
          }

          $lstProjectConfigs
            .append(
              $('<option></option>')
                .attr('value', projectConfiguration)
                .text(projectConfiguration));
        });

        if (g_initialSelection && g_initialSelection.projectConfigurationName) {
          valueToSelect = g_initialSelection.projectConfigurationName;
        }

        if (valueToSelect !== null) {
          var projectConfigsElement = domHelper.getProjectConfigsElement();

          projectConfigsElement.val(valueToSelect);

          if (projectConfigsElement.val() === null) {
            toastr.error('No project configuration named \'' + valueToSelect + '\'.');
            return;
          }

          projectConfigsElement.trigger('change');
        }

        if (onFinishedCallback) {
          onFinishedCallback();
        }
      });
}

function loadProjectConfigurationBuilds(projectName, projectConfigurationName, onFinishedCallback) {
  clearProjectConfigurationBuilds();

  $.getJSON(
    g_AppPrefix + 'Api/GetProjectConfigurationBuilds?projectName=' + encodeURIComponent(projectName) + '&projectConfigurationName=' + encodeURIComponent(projectConfigurationName),
    function (data) {
      clearProjectConfigurationBuilds();

      $.each(data.projectConfigurationBuilds, function (i, val) {
        var $lstProjectConfigBuilds = $('#lst-project-config-builds');

        $lstProjectConfigBuilds
          .append(
            $('<option></option>')
              .attr('value', val.Id)
              .text(val.StartDate + ' | ' + val.StartTime + ' | ' + val.Number));
      });

      if (onFinishedCallback) {
        onFinishedCallback();
      }
    });
}

function startDiagnosticMessagesLoader() {
  loadNewDiagnosticMessages();

  setTimeout(
    function () {
      startDiagnosticMessagesLoader();
    },
    g_DiagnosticMessagesLoaderInterval);
}

function loadNewDiagnosticMessages() {
  $.getJSON(
    g_AppPrefix + 'Api/GetDiagnosticMessages?lastSeenMaxMessageId=' + g_lastSeenMessageId,
    function (data) {
      $.each(data.messages, function (i, val) {
        if (val.MessageId > g_lastSeenMessageId) {
          logMessage(val.Message, val.Type);
          g_lastSeenMessageId = val.MessageId;
        }
      });
    });
}

function domHelper() { }

domHelper.getProjectsElement = function () {
  return $('#lst-projects');
};

domHelper.getProjectConfigsElement = function () {
  return $('#lst-project-configs');
};

domHelper.getEnvironmentsElement = function () {
  return $('#lst-environments');
};

domHelper.getMachinesElement = function () {
  return $('#lst-machines');
};

domHelper.getSelectedMachines = function () {
  return $.map($('#lst-machines option:selected'), function (item) { return $(item).val(); });
};

domHelper.showError = function (xhr) {
  if (xhr.readyState === 0 || xhr.status === 0) {
    return;
  }

  $('#errorMsg').html('<strong>Error</strong>, Status Code:' + xhr.status + ' ' + xhr.statusText + ' ' + xhr.responseText);
  $('#errorMsg').show();
  $('#main-container').hide();
};

function getSelectedTargetEnvironmentName() {
  return $('#lst-environments').val();
}

function getSelectedProjectName() {
  return $('#lst-projects').val();
}

function getSelectedProjectConfigurationName() {
  return $('#lst-project-configs').val();
}

function getSelectedProjectConfigurationBuildId() {
  return $('#lst-project-config-builds').val();
}

function getSelectedTargetMachines() {
  return domHelper.getSelectedMachines();
}

function isOnlyDeployable() {
  return $('#chb-deployable-projects').is(':checked');
}

function clearEnvironments() {
  $('#lst-environments').empty();
}

function clearProjects() {
  $('#lst-projects').empty();
}

function clearProjectConfigurations() {
  $('#lst-project-configs').empty();
}

function clearProjectConfigurationBuilds() {
  $('#lst-project-config-builds').empty();
}

function clearTargetMachines() {
  domHelper.getMachinesElement().empty();
}

function rememberTargetEnvironmentName() {
  var targetEnvironmentName = getSelectedTargetEnvironmentName();

  if (!targetEnvironmentName) {
    return;
  }

  setCookie(
    g_TargetEnvironmentCookieName,
    targetEnvironmentName,
    g_TargetEnvironmentCookieExpirationInDays);
}

function selectEnvironment(environmentName) {
  var environmentsElement = domHelper.getEnvironmentsElement();

  environmentsElement.val(environmentName);

  if (domHelper.getEnvironmentsElement().val() === null) {
    toastr.error('No environment named \'' + environmentName + '\'.');
    return;
  }

  environmentsElement.trigger('change');
}

function restoreRememberedTargetEnvironmentName() {
  var selectFirstValueFunc =
    function () {
      var firstVal = domHelper.getEnvironmentsElement().find('option').eq(0).val();

      if (firstVal !== undefined) {
        domHelper.getEnvironmentsElement().val(firstVal);
        domHelper.getEnvironmentsElement().trigger('change');
      }
    };

  var cookie = getCookie(g_TargetEnvironmentCookieName);

  if (cookie === null) {
    selectFirstValueFunc();
    return;
  }

  domHelper.getEnvironmentsElement().val(cookie);

  if (domHelper.getEnvironmentsElement().val() === null) {
    selectFirstValueFunc();
    return;
  }

  domHelper.getEnvironmentsElement().trigger('change');
}

function setCookie(c_name, value, exdays) {
  var exdate = new Date();

  exdate.setDate(exdate.getDate() + exdays);

  var c_value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());

  document.cookie = c_name + "=" + c_value;
}

function getCookie(c_name) {
  var i, x, y, ARRcookies = document.cookie.split(";");

  for (i = 0; i < ARRcookies.length; i++) {
    x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
    y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
    x = x.replace(/^\s+|\s+$/g, "");

    if (x == c_name) {
      return unescape(y);
    }
  }

  return null;
}

function alternateTableRows(tableId) {
  $('#' + tableId + ' tr:even')
    .each(function () {
      $(this).addClass('even');
    });
}

function logMessage(message, type) {
  var $txtLogs = $('#txt-logs');
  var $logMsg = $('<div></div>');

  $logMsg.text('>> ' + message);
  $logMsg.addClass('log-msg');

  var typeLower = type.toLowerCase();

  if (typeLower === 'trace') {
    $logMsg.addClass('log-msg-trace');
  }
  else if (typeLower == 'info') {
    $logMsg.addClass('log-msg-info');
  }
  else if (typeLower == 'warn') {
    $logMsg.addClass('log-msg-warn');
  }
  else if (typeLower == 'error') {
    $logMsg.addClass('log-msg-error');
  }

  $txtLogs.append($logMsg);
  $txtLogs.scrollTop($txtLogs[0].scrollHeight - $txtLogs.height());
}

function clearLogs() {
  var $txtLogs = $('#txt-logs');

  $txtLogs.find('*').remove();
}

// returns true if there was no error
function handleApiErrorIfPresent(data) {
  if (!data.Status || data.Status.toLowerCase() === 'fail') {
    var message;

    if (data.ErrorMessage) {
      message = data.ErrorMessage;
    } else {
      message = '(no error message)';
    }

    logMessage('Error: ' + message, 'error');

    return false;
  }

  return true;
}

function getProjectVersion() {
  var projectName = getSelectedProjectName();
  var targetEnvironmentName = getSelectedTargetEnvironmentName();

  if (!projectName || !targetEnvironmentName) {
    toastr.error('Select project and environment!');
    return;
  }

  logMessage('Getting version info for project \'' + projectName + '\' on environment \'' + targetEnvironmentName + '\'...', 'info');

  $.ajax({
    url: g_AppPrefix + 'Api/GetProjectMetadata',
    type: "GET",
    data: {
      projectName: projectName,
      environmentName: targetEnvironmentName
    },
    traditional: true,
    success: function (data) {
      if (!handleApiErrorIfPresent(data)) {
        return;
      }

      if (!data.ProjectVersions || data.ProjectVersions.length === 0) {
        logMessage('No version info for project \'' + data.ProjectName + '\' on environment \'' + data.EnvironmentName + '\'.', 'info');
        return;
      }

      $(data.ProjectVersions).each(function (i, projectVersion) {
        logMessage('Version of project \'' + data.ProjectName + '\' on environment \'' + data.EnvironmentName + '\' on machine \'' + projectVersion.MachineName + '\' is: \'' + projectVersion.ProjectVersion + '\'.', 'info');
      });
    }
  });
}

function kickAss() {
  var s = document.createElement('script');

  s.type = 'text/javascript';
  document.body.appendChild(s);
  s.src = '//hi.kickassapp.com/kickass.js';

  return false;
}

function setupSignalR(collectCredentialsDialog, collectScriptsToRunDialog, collectProjectDependenciesDialog) {
  var deploymentHub = $.connection.deploymentHub;
  var diagnosticMessagesHub = $.connection.diagnosticMessagesHub;

  setupDiagnosticMessagesHub(diagnosticMessagesHub);
  
  deploymentHub.client.promptForCredentials = function (message) {
    collectCredentialsDialog.showDialog(message);
  };

  deploymentHub.client.cancelPromptForCredentials = function () {
    collectCredentialsDialog.closeDialog();
  };

  deploymentHub.client.promptForScriptsToRun = function (message) {
    collectScriptsToRunDialog.showDialog(message);
  };

  deploymentHub.client.cancelPromptForScriptsToRun = function () {
    collectScriptsToRunDialog.closeDialog();
  };

  deploymentHub.client.promptForProjectDependencies = function (message) {
    collectProjectDependenciesDialog.showDialog(message);
  };

  deploymentHub.client.cancelPromptForProjectDependencies = function () {
    collectProjectDependenciesDialog.closeDialog();
  };

  $.connection.hub.start();
}

function setupDiagnosticMessagesHub(diagnosticMessagesHub) {
  diagnosticMessagesHub.client.onDiagnosticMessage = function(message) {
    console.debug(message);
  }
}

var CollectCredentialsDialog = (function () {
  function CollectCredentialsDialog() {
    var self = this;
    $('#dlg-collect-credentials-ok')
      .click(function () {
        var deploymentId = $('#dlg-collect-credentials-deployment-id').val();
        var password = $('#dlg-collect-credentials-password').val();

        if (password === '') {
          alert('You have to enter the password.');
          return;
        }

        $.ajax({
          url: g_AppPrefix + 'InternalApi/OnCredentialsCollected',
          type: "POST",
          data: {
            deploymentId: deploymentId,
            password: password,
          },
          traditional: true
        });

        self.closeDialog();
      });

    $('#dlg-collect-credentials')
      .on(
        'shown',
        function () {
          $('#dlg-collect-credentials-password').focus();
        });
  };

  CollectCredentialsDialog.prototype.showDialog = function (message) {
    $('#dlg-collect-credentials-deployment-id').val(message.deploymentId);
    $('#dlg-collect-credentials-project-name').html(message.projectName);
    $('#dlg-collect-credentials-project-configuration-name').html(message.projectConfigurationName);
    $('#dlg-collect-credentials-target-environment-name').html(message.targetEnvironmentName);
    $('#dlg-collect-credentials-machine-name').val(message.machineName);
    $('#dlg-collect-credentials-username').val(message.username);
    $('#dlg-collect-credentials-password').val('');

    $('#dlg-collect-credentials').modal('show');
  };

  CollectCredentialsDialog.prototype.closeDialog = function () {
    $('#dlg-collect-credentials-deployment-id').val('');
    $('#dlg-collect-credentials-project-name').html('');
    $('#dlg-collect-credentials-project-configuration-name').html('');
    $('#dlg-collect-credentials-target-environment-name').html('');
    $('#dlg-collect-credentials-machine-name').val('');
    $('#dlg-collect-credentials-username').val('');
    $('#dlg-collect-credentials-password').val('');

    $('#dlg-collect-credentials').modal('hide');
  };

  return CollectCredentialsDialog;
})();

var CollectScriptsToRunDialog = (function () {
  function CollectScriptsToRunDialog(isMultiple) {
    var self = this;

    self.isMultiple = isMultiple;

    // TODO LK: add click handler for cancel button. We should send cancel command to service asap and not wait for timeout.
    $('#dlg-collect-scripts-ok')
      .click(function () {
        var deploymentId = $('#dlg-collect-scripts-deployment-id').val();
        var selectedScripts = $('#dlg-collect-scripts-select').val();

        if (selectedScripts === '') {
          alert('Scripts were not selected!');
          return;
        }

        $.ajax({
          url: g_AppPrefix + 'InternalApi/OnScriptsToRunCollected',
          type: "POST",
          data: {
            deploymentId: deploymentId,
            selectedScripts: selectedScripts,
            isMultiselect: isMultiple
          },
          traditional: true
        });

        self.closeDialog();
      });

    $('#dlg-collect-scripts-cancel')
      .click(function () {
        var deploymentId = $('#dlg-collect-scripts-deployment-id').val();

        $.ajax({
          url: g_AppPrefix + 'InternalApi/OnScriptsToRunCanceled',
          type: "POST",
          data: {
            deploymentId: deploymentId
          },
          traditional: true
        });

        self.closeDialog();
      });
  };

  CollectScriptsToRunDialog.prototype.showDialog = function (message) {
    $('#dlg-collect-scripts-deployment-id').val(message.deploymentId);
    $('#dlg-collect-scripts-project-name').html(message.projectName);
    $('#dlg-collect-scripts-project-configuration-name').html(message.projectConfigurationName);
    $('#dlg-collect-scripts-select').html("");

    $.each(message.scriptsToRun, function (index, scriptToRun) {
      $('#dlg-collect-scripts-select').append('<option value=' + scriptToRun + '>' + scriptToRun + '</option>');
    });

    if (this.isMultiple) {
      $('#dlg-collect-scripts-select').attr('multiple', 'multiple');
    } else {
      $('#dlg-collect-scripts-select option:last-child').attr('selected', 'selected');
    }

    $('#dlg-collect-scripts').modal('show');
  };

  CollectScriptsToRunDialog.prototype.closeDialog = function () {
    $('#dlg-collect-scripts-deployment-id').val('');
    $('#dlg-collect-scripts-project-name').html('');
    $('#dlg-collect-scripts-project-configuration-name').html('');
    $('#dlg-collect-scripts-select').find('option').remove();

    $('#dlg-collect-scripts').modal('hide');
  };


  return CollectScriptsToRunDialog;
})();


/* Project dependencies */
var CollectProjectDependenciesDialog = (function () {
  function CollectProjectDependenciesDialog() {
    var self = this;

    var collectDependenciesSubmit = $('#dlg-collect-dependencies-ok');
    collectDependenciesSubmit.unbind("click");

    var collectDependenciesSkip = $('#dlg-collect-dependencies-skip');
    collectDependenciesSkip.unbind("click");

    var collectDependenciesCancel = $('#dlg-collect-dependencies-cancel');
    collectDependenciesCancel.unbind("click");

    collectDependenciesSubmit
      .click(function () {
        var deploymentId = $('#dlg-collect-dependencies-deployment-id').val();
        
        var selectedDependencies = [];
        
        var selectedInputs = $('#dlg-collect-dependencies-grid').find("input:checked");

        selectedInputs.each(function () {
          var dependentPrj = {
            ProjectName: $(this).attr("name"),
            BranchName: $(this).siblings("input[name='branch-name']").val(),
            BuildNumber: $(this).siblings("input[name='build-number']").val()
          };
          selectedDependencies.push(dependentPrj);
        });

        var postData = {
          DeploymentId: deploymentId,
          DependenciesToDeploy: selectedDependencies
        };

        $.ajax({
          url: g_AppPrefix + 'InternalApi/OnDependenciesToRunCollected',
          type: "POST",
          data: JSON.stringify(postData),
          traditional: true,
          contentType: 'application/json; charset=utf-8',
        });

        self.closeDialog();
      });

    collectDependenciesSkip
      .click(function () {
        var deploymentId = $('#dlg-collect-dependencies-deployment-id').val();

        $.ajax({
          url: g_AppPrefix + 'InternalApi/OnDependenciesToRunSkipped',
          type: "POST",
          data: {
            deploymentId: deploymentId
          },
          traditional: true
        });

        self.closeDialog();
      });

    collectDependenciesCancel
      .click(function () {
        var deploymentId = $('#dlg-collect-dependencies-deployment-id').val();

        $.ajax({
          url: g_AppPrefix + 'InternalApi/OnDependenciesToRunCancelled',
          type: "POST",
          data: {
            deploymentId: deploymentId
          },
          traditional: true
        });

        self.closeDialog();
      });
  };

  CollectProjectDependenciesDialog.prototype.showDialog = function (message) {
    $('#dlg-collect-dependencies-deployment-id').val(message.deploymentId);

    var projectsGrid = $('#dlg-collect-dependencies-grid');
    projectsGrid.empty();
    
    $.each(message.dependentProjects, function (index, dependency) {
      projectsGrid.append(
        '<div class="row">' +
        '<div class="col-sm-7">' +
        '<input type="checkbox" id="dlg-collect-dependencies-' + dependency.ProjectName + '" name="' + dependency.ProjectName + '" checked="checked"' +
        '<label for="dlg-collect-dependencies-' + dependency.ProjectName + '">' +
        dependency.ProjectName +
        '</label>' +
        '<input type="hidden" name="branch-name" value="' + dependency.BranchName + '">' +
        '<input type="hidden" name="build-number" value="' + dependency.BuildNumber + '">' +
        '</div>' +
        '<div class="col-sm-3">' +
        dependency.BranchName +
        '</div>' +
        '<div class="col-sm-2">' +
        dependency.BuildNumber +
        '</div>' +
        '</div>'
        );
    });
    
    $('#dlg-collect-dependencies').modal('show');
  };

  CollectProjectDependenciesDialog.prototype.closeDialog = function () {
    $('#dlg-collect-dependencies-deployment-id').val('');
    $('#dlg-collect-dependencies-ulist').empty();

    $('#dlg-collect-dependencies').modal('hide');
  };

  return CollectProjectDependenciesDialog;
})();


$(document).ready(function () {
  // do nothing
});