var UberDeployer = UberDeployer || {};

UberDeployer.EnvDeploy = function() {
  var _initialSelection = null;

  var _projects = null;

  var _self = this;

  var doDeployEnv = function() {
    var targetEnvironmentName = getSelectedTargetEnvironmentName();

    $.ajax({
      url: g_AppPrefix + 'EnvDeployment/DeployAll',
      type: "POST",
      data: {
        environmentName: targetEnvironmentName,        
        projectNames: Object.keys(_projects)
      },
      traditional: true,
      success: function(data) {
        handleApiErrorIfPresent(data);
      }
    });
  };

  var loadProjectsForCurrentEnvironmentDeploy = function() {
    clearProjects();

    $.getJSON(g_AppPrefix + 'EnvDeployment/GetProjectsForEnvironmentDeploy', { environmentName: getSelectedTargetEnvironmentName() })
      .done(
        function(data) {
          _projects = [];
          clearProjects();

          $.each(data.projects, function(i, val) {
            _projects[val.Name] = new Project(val.Name, val.Type, val.AllowedEnvironmentNames);

            domHelper.getProjectsElement()
              .append(
                $('<option></option>')
                  .attr('value', val.Name)
                  .text(val.Name));
          });
        });
  };

  var setupSignalR = function() {
    var deploymentHub = $.connection.deploymentHub;

    deploymentHub.client.connected = function() {
    };
    deploymentHub.client.disconnected = function() {
    };

    deploymentHub.client.promptForCredentials =
      function(message) {
        showCollectCredentialsDialog(
          message.deploymentId,
          message.projectName,
          message.projectConfigurationName,
          message.targetEnvironmentName,
          message.machineName,
          message.username);
      };  

    deploymentHub.client.cancelPromptForCredentials =
      function() {
        closeCollectCredentialsDialog();
      };

    $.connection.hub.start();
  };

  var showCollectCredentialsDialog = function(deploymentId, projectName, projectConfigurationName, targetEnvironmentName, machineName, username) {
    $('#dlg-collect-credentials-deployment-id').val(deploymentId);
    $('#dlg-collect-credentials-project-name').html(projectName);
    $('#dlg-collect-credentials-project-configuration-name').html(projectConfigurationName);
    $('#dlg-collect-credentials-target-environment-name').html(targetEnvironmentName);
    $('#dlg-collect-credentials-machine-name').val(machineName);
    $('#dlg-collect-credentials-username').val(username);
    $('#dlg-collect-credentials-password').val('');

    $('#dlg-collect-credentials').modal('show');
  };

  var closeCollectCredentialsDialog = function() {
    $('#dlg-collect-credentials-deployment-id').val('');
    $('#dlg-collect-credentials-project-name').html('');
    $('#dlg-collect-credentials-project-configuration-name').html('');
    $('#dlg-collect-credentials-target-environment-name').html('');
    $('#dlg-collect-credentials-machine-name').val('');
    $('#dlg-collect-credentials-username').val('');
    $('#dlg-collect-credentials-password').val('');

    $('#dlg-collect-credentials').modal('hide');
  };

  var setupCollectCredentialsDialog = function() {
    $('#dlg-collect-credentials-ok')
      .click(function() {
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

        closeCollectCredentialsDialog();
      });

    $('#dlg-collect-credentials')
      .on(
        'shown',
        function() {
          $('#dlg-collect-credentials-password').focus();
        });
  };  

  return {
    initializeEnvDeploymentPage : function(initData) {
      _initialSelection = initData.initialSelection;

      setupSignalR();
      setupCollectCredentialsDialog();

      $.ajaxSetup({
        'error': function(xhr) {
          domHelper.showError(xhr);
        }
      });

      $('#btn-deployEnv').click(function() {
        doDeployEnv();
      });

      loadEnvironments(function() {
        if (g_initialSelection && g_initialSelection.targetEnvironmentName) {
          selectEnvironment(g_initialSelection.targetEnvironmentName);
        } else {
          restoreRememberedTargetEnvironmentName();
        }
      });

      domHelper.getEnvironmentsElement().change(function() {
        if (!g_initialSelection || !g_initialSelection.targetEnvironmentName) {
          rememberTargetEnvironmentName();
        }

        loadProjectsForCurrentEnvironmentDeploy();
      });

      startDiagnosticMessagesLoader();
    }
  };
}();