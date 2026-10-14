
var app = angular.module('InfinityLab');

app.controller('CRMReportsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
      
        $scope.Entity = {
            "id": 0,
            "name": ""
         
        };

        $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ErrorStyle = "success";
        $scope.SearchResult = [];

    
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            fromdate: "",
            todate: "",
            married: false
        }

        $scope.Clear = function () {
            $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Send = function () {

            request = {
                "Item": $scope.Customer,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("SMS SENT Successfully.!");
                    $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

        $scope.SearchBirthday = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchBirthday",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.BirthDayConfig();
            }).error(function (result) {
            });

        }

        $scope.SearchAnniversary = function () {
            $scope.SearchCondition.married = true;

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchBirthday",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }

        $scope.BirthDayConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '300px', '100%', true, true, true,0);

            igrid.AddSimpleColumn('name', 'Customer Name', 300);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 200);
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SearchResult);


        }

        $scope.SendSMS = function (list) {
            var items = [];

            var dialog = ngDialog.open({
                template: 'SendSms',
                data: { 'scope': $scope, 'list': items },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                  
                    $scope.Send = function (item) {

                        var poRequest = {
                            'SendSms': $scope.PopupList, isactive: true,
                            version: 1,
                            supplierid: $scope.supplierid,
                            podate: $scope.podate,
                            expectedate: $scope.expectedate
                        };
                        var request = {
                            "item": poRequest,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "PurchaseorderService/Save",
                            data: srequest
                        }).success(function (result) {

                            if (result != null && result.id > 0) {
                                $scope.ngDialogData.result = result;
                                $scope.closeThisDialog($scope.ngDialogData);
                                $rootScope.Toaster.Info("PO generated successfully with PO No : " + result.pono);
                            }
                            else {
                                $scope.ErrorMessage = result.ErrorMessage;
                                $scope.ErrorStyle = "danger";
                            }

                        }).error(function (result) {
                        });

                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {

                }
            });
        }


        $timeout(function () {
            $scope.setFocusfirst();
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "AgentService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {
                        $scope.ShowSearch = false;
                        $scope.Agent = result[0];
                    }

                }).error(function (result) {
                });
            }
        }, 100);

    }]);