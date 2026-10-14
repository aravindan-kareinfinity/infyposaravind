
var app = angular.module('InfinityLab');

app.controller('CountryController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    function (UserDetails, $scope, $http, $location, $rootScope, $window) {

        $rootScope.AccordionName = $location.path();

        $scope.SpecimenList = [{ 'ID': 1, 'Name': 'Test - 1' }, { 'ID': 2, 'Name': 'Test - 2' }];
        $scope.DepartmentList = [{ 'ID': 1, 'Name': 'Test - 1' }, { 'ID': 2, 'Name': 'Test - 2' }];
        $scope.ResultValueTypeList = [{ 'ID': 'Text', 'Name': 'Text' }, { 'ID': 'Number', 'Name': 'Number' }, { 'ID': 'MultilineText', 'Name': 'Multiline Text' },
        { 'ID': 'SubTitle', 'Name': 'Sub Title' }, { 'ID': 'Information', 'Name': 'Information' }, { 'ID': 'SingleSelection', 'Name': 'Single Selection' }];
        $scope.RangeModes = ['Basic', 'Complex'];
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.ErrorMessage = "";
        $scope.Country = {
            Id: 0,
            Code: "",
            Name: "",
            AdditionalInfo: "",
            SortingIndex: "",
            version: "1"
        };

        $scope.SearchCondition = {
            Code: "",
            Name: ""
        }

        $scope.Save = function () {

            request = {
                "Item": $scope.Tax,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TaxService/Save", 
                data: srequest
            }).success(function (result) {

                if (result.Success) {
                    $scope.ErrorMessage = "Successfully saved";
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                }

            }).error(function (result) {
            });

        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "delete") {
                request = {
                    "Item": item,
                    "SecurityToken": "String content"
                }
                $http({
                    method: 'POST',
                    url: "TaxService/Delete",
                    data: srequest
                }).success(function (result) {

                    if (result.Success) {
                        $scope.ErrorMessage = "Successfully deleted";
                    }
                    else {
                        $scope.ErrorMessage = result.ErrorMessage;
                    }

                }).error(function (result) {

                });
            }
            else if (action == "edit") {
                $scope.ShowSearch = false;
                $scope.Tax = item;
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CountryService/Search",
                data: srequest
            }).success(function (result) {
                alert(result.items.length);
                $scope.SearchResult = result.items;
            }).error(function (result) {
            });

        }
}]);