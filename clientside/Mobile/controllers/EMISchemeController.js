
var app = angular.module('InfinityLab');

app.controller('EMISchemeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Navigation.SetMode('SEARCH');

        $scope.LoanProviders = restService.Get('ReferencelistService/LOANPROVIDER', true).then(function (results) {
            $scope.LoanProviders = results;
        });
        

        $scope.Entity =
            {
                id: "",
                supplierid: "",
                productid: "",
                itemid: "",
                status: "",
                fromdate: new Date(),
                todate: new Date(),
                minamt: "",
                maxamt: "",
                items: [],
                isactive: true,
                createdby: "",
                modifiedby: "",
                version: "",
                createdon: new Date(),
                modifiedon: new Date()
            };

        $scope.EMIScheme = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ShowSearch = false;

        $scope.DeleteItem = function (item) {
            var index = $scope.EMIScheme.items.indexOf(item);
            $scope.EMIScheme.items.splice(index, 1);
        }

      

        $scope.New = function () {
            $scope.EMIScheme = (JSON.parse(JSON.stringify($scope.Entity)));

        }
        $scope.New();
        $scope.ShowSearch = false;
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.igrid = null;
        $scope.ConfigGrid = function () {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Scheme', 150);
            igrid.AddReferenceColumn('loanproviderid', 'Provider', 200, $scope.LoanProviders);
            igrid.AddSimpleColumn('rateofinterest', 'Interest %', 100);
            igrid.AddSimpleColumn('tenure', 'Tenure', 50);
            igrid.AddSimpleColumn('minimumadvancemonth', 'Adv. Months', 70);
            igrid.AddCustomColumn('dbdpercentage', 'DBD', 60, function (item) {
                if (item.dbdvalue > 0)
                    return item.dbdvalue;
                return item.dbdpercentage + '%';
            });

            igrid.AddCustomColumn('documentchargepercentage', 'Document', 60, function (item) {
                if (item.documentcharge > 0)
                    return item.documentcharge;
                return item.documentchargepercentage + '%';
            });

            igrid.AddCustomColumn('minimumvalue', 'Bill Amount', 100, function (item) {
                return item.minimumvalue + ' - ' + item.maximumvalue;
            });

            igrid.AddFormatedColumn('validfrom', 'Valid from', 60, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validto', 'Valid to', 60, 'dd-MM-yyyy');
            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $scope.Delete = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length = 1) {

                var request = {
                    "Item":  seletedRows[0] ,
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "EmischemesService/Delete",
                    data: srequest
                }).success(function (result) {
                    if (result) {
                        var index = $scope.SearchResult.indexOf(this.igrid.GetSelectedRows());
                        $scope.SearchResult.splice(index, 1);
                        $scope.igrid.UpdateData($scope.SearchResult);
                    }
                }).error(function (result) {

                });

            }
            else {
                $rootScope.Toaster.Info("Please select one item to delete");
            }
        }

        $scope.Edit = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length = 1) {

                var request = {
                    "Item": { 'id': seletedRows[0].id},
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "EmischemesService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length == 1) {
                        $scope.EMIScheme = result[0];
                        $scope.ShowSearch = false;
                    }
                }).error(function (result) {

                });
            }
            else {
                $rootScope.Toaster.Info("Please select one item to edit");
            }

        }

        $scope.SearchResult = [];
        $scope.SearchRegister = function () {
            if (typeof ($scope.SearchRequest) == 'undefined' || $scope.SearchRequest == null)
                $scope.SearchRequest = {};

            $scope.SearchRequest.hideitems = true;
            var request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmischemesService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.igrid == null)
                    $scope.ConfigGrid();
                $scope.SearchResult = result;
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        }

        $scope.Save = function () {

            var request = {
                "Item": $scope.EMIScheme,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmischemesService/Save",
                data: request
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Order Saved Successfully");
                    $scope.EMIScheme = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }


        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);


    }]);

