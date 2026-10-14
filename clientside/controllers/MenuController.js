
var app = angular.module('InfinityLab');

app.controller('MenuController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout','$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $stateParams) {
        $rootScope.AccordionName = $location.path();
        $scope.CanChangeLockDate = false;
        $scope.ApplicationSettings = null;
        $scope.sub_wh = false;
        $scope.sub_sales = false;
        $scope.sub_fin = false;
        $scope.sub_store = false;
        $scope.sub_anal = false;
        $scope.sub_hr = false;
        $scope.sub_pro = false;
        $scope.sub_set = false;
        $scope.sub_all = true;
        $scope.showsystemcommands = false;
        $scope.newschema = null;
        $scope.newscommand = null;
        $scope.RemoveLockDate = function () {
            $scope.payroll_locakdate = new Date(1, 1, 1);
            $scope.SetLockDate();
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.newscommand = $scope.tnewscommand;
            $scope.tnewscommand = null;
        }

        $scope.SetLockDate = function () {
            $scope.ApplicationSettings.key = 'payroll_locakdate';
            $scope.ApplicationSettings.appsettings.payroll_locakdate = $scope.payroll_locakdate;
            var srequest = JSON.stringify({
                "Item": $scope.ApplicationSettings,
                "securitytoken": "String content"
            });

            $http({
                method: 'POST',
                url: "ConfigurationService/SetValue",
                data: srequest
            }).success(function (result) {
                
            });
        }

        $scope.ShowLockDate = function () {
            $scope.CanChangeLockDate = true;

            var srequest = JSON.stringify({
                "Item": { 'locationid': -9, 'key': 'APPLICATIONSETTINGS' },
                "securitytoken": "String content"
            });

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.ApplicationSettings = result[0];
                    $scope.payroll_locakdate = result[0].appsettings.payroll_locakdate
                }
            });
        }

        $scope.menuclicked = function (menuname) {
            if ($scope[menuname]) {
                $scope.sub_all = true;
                $scope.sub_wh = false;
                $scope.sub_crm = false;
                $scope.sub_sales = false;
                $scope.sub_fin = false;
                $scope.sub_store = false;
                $scope.sub_anal = false;
                $scope.sub_hr = false;
                $scope.sub_pro = false;
                $scope.sub_set = false;
                $scope.sub_mfg = false;
                $scope.showsystemcommands = false;
            }
            else {
                $scope.sub_all = false;
                $scope.sub_wh = false;
                $scope.sub_sales = false;
                $scope.sub_fin = false;
                $scope.sub_store = false;
                $scope.sub_anal = false;
                $scope.sub_hr = false;
                $scope.sub_pro = false;
                $scope.sub_set = false;
                $scope.sub_mfg = false;
                $scope.sub_crm = false;
                $scope.showsystemcommands = false;
                $scope[menuname] = true;
            }
        }

        $('.main-sidebar').removeClass('sidebar-expand');

        if (typeof ($stateParams.menu) != 'undefined' && $stateParams.menu != null) {
            $scope.menuclicked($stateParams.menu);
        }

        $scope.SyncOffline = function () {

            request = {
                "Item": {},
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SyncController/SyncOffline",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result);
            }).error(function (result) {

            });
        }

        $scope.Runcommand = function () {
            restService.Get('File/Runcommand/' + $scope.UserInfo.OrganizationID + '/' + $scope.newscommand).then(function (result) {
                $scope.ProcessTitle = 'Promotion Simulation';
                $scope.StatusID = result.identifier;
                $scope.UploadedingStatus = result;
                $scope.ProcessStarted = true;
                $scope.ProgressValue = 0;
                $scope.migrationtimer = $timeout(function () {
                    $scope.CheckStatus();
                }, 2000);
            });
        }

        $scope.ProcessTitle = '';
        $scope.StartProcess = function (action,title) {
            var request = {
                'action': action
            };
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "File/Process",
                data: srequest
            }).success(function (result) {
                $scope.ProcessTitle = title;
                $scope.StatusID = result.identifier;
                $scope.UploadedingStatus = result;
                $scope.ProcessStarted = true;
                $scope.ProgressValue = 0;
                $scope.migrationtimer = $timeout(function () {
                    $scope.CheckStatus();
                }, 2000);
            }).error(function (result) {
            });
        }

        $scope.CheckStatus = function () {
            $scope.ShowCommandResult = false;
            restService.Get('File/ProcessStatus/' + $scope.UserInfo.OrganizationID + '/' + $scope.StatusID).then(function (result) {
                $scope.UploadedingStatus = result;
                $scope.ProgressValue = result.percentage;
                if (!result.hascompleted) {
                    $scope.migrationtimer = $timeout(function () {
                        $scope.CheckStatus();
                    }, 2000);
                } else {
                    if ($scope.ProcessTitle == 'Promotion Simulation') {
                        $scope.ShowCommandResult = true;
                        //$timeout(function () {
                        //    var link = document.createElement('a');
                        //    link.target = "_blank";
                        //    link.href = 'File/resultcommand/' + $scope.UserInfo.OrganizationID + '/' + $scope.StatusID;
                        //    document.body.appendChild(link);
                        //    link.click();
                        //    link.remove();
                        //}, 10);
                    }
                }
            });
        }

        $scope.migrationtimer = null;
        $scope.ProcessStarted = false;
        $scope.ProgressValue = 0;
        $scope.$on(
            "$destroy",
            function (event) {
                if ($scope.migrationtimer != null)
                    $timeout.cancel($scope.migrationtimer);
            }
        );

    
        $scope.CreateOffline = function () {

            request = {
                "Item": {},
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SyncController/RestAutoNumber",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result);
            }).error(function (result) {

            });
        }

        $scope.ShowCommandProcess = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
                $scope.ShowCommandProcess = $rootScope.UserInfo.appsettings.HasCommandLib;

                $rootScope.ShowDeliveryChallan = $rootScope.UserInfo.appsettings.system_supportdc;
                $rootScope.ShowEstimateBill = $rootScope.UserInfo.appsettings.system_supportestimate;
                $rootScope.ShowProductDesign = $rootScope.UserInfo.appsettings.system_disabledesigneditable;
                $rootScope.ShowDeliveryRack = $rootScope.UserInfo.appsettings.system_supportdeliveryrack;
                $rootScope.ShowEcommerce = $rootScope.UserInfo.appsettings.unicommerce;

                if ($scope.ShowCommandProcess) {
                    $scope.CommandProcess = $rootScope.UserInfo.appsettings.CommandLibClass + ', ' +
                        $rootScope.UserInfo.appsettings.CommandLibPath;
                }
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }

        $timeout(function () {
            $scope.LoadApplicationSetting();
        }, 10);
    }]);