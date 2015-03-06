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
        function (data) {
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