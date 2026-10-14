
var app = angular.module('InfinityLab');

app.controller('LoanReconciliationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();

        $scope.Modes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'PAID' }, { 'id': 2, 'name': 'PENDING' }];

        $scope.LoanProviderList = restService.Get('ReferencelistService/LOANPROVIDER', true).then(function (results) {
            $scope.LoanProviderList = results;
        });

        $scope.Entity = {
            "id": 0,
            "paid": "",
            "allocated": "",
            "discount": "",
            "receiptno": "",
            "remarks": "",
            "settlementon": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "items": { "items": [] },
        };
        $scope.LoanReconciliation = (JSON.parse(JSON.stringify($scope.Entity)));


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.item = {};
        $scope.paid = 0;

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.LoanReconciliation = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.paychanged = function () {
            $scope.paid = 0;

            $.each($scope.SearchResult, function (k, v) {
                $scope.paid += v.newpaid;
            });
        }
        $scope.SavePayment = function () {

            if (typeof ($scope.LoanReconciliation.items) == 'undefined' || $scope.LoanReconciliation.items == null)
                $scope.LoanReconciliation.items = [];
            
            $.each($scope.SearchResult, function (k, v) {
                $scope.LoanReconciliation.items.push({
                    'paid': $scope.item.newpaid, 'discount': $scope.item.newdiscount, 'id': v.id, 'referenceno': v.referenceno,
                    'receiptno': $scope.LoanReconciliation.receiptno,
                    'settlementon': $scope.LoanReconciliation.settlementon, 'remarks': $scope.LoanReconciliation.remarks
                });
            });

            request = {
                "Item": $scope.LoanReconciliation,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LoansettlementService/Save",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Loan Updated Successfully.!");
                $scope.LoanReconciliation = (JSON.parse(JSON.stringify($scope.Entity)));

            }).error(function (result) {
            });

        }

        $scope.LoanReconciliation = {};

        $scope.SearchItems = function () {

            request = {
                "Item": $scope.LoanReconciliation,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LoansettlementService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }


    }]);