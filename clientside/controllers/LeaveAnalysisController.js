
var app = angular.module('InfinityLab');

app.controller('LeaveAnalysisController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService) {


        $scope.SearchRef = restService.SearchReference;
        $scope.ShowNew = false;
        $scope.SearchResult = [];

        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
                            { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
                            { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
                            { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
                            { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
                            { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";
        $scope.Entity = {
            "isactive": true,
            "createdby": "",
            "modifiedby": "",
            "version": "",
            "createdon": "",
            "modifiedon": "",
            "name": "",
            "description": "",
            "holidayon": "",
            "id": 0,
            "holidaytype": "",
            "payable": "",

        };


        $scope.LeaveAnalysis = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.SearchCondition = {
            name: "",
            description: "",
            holidayon: "",
            holidaytype: "",
            institutionid: 0,
            academicid: 0
        }

        $scope.Save = function () {

            request = {
                "Item": $scope.Holiday,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "HolidayService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.Holiday = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.Search();
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {

                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    confirm: function () {

                        request = {
                            "Item": item,
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "HolidayService/Delete",
                            data: srequest
                        }).success(function (result) {

                            if (result) {
                                $scope.ErrorMessage = "Successfully deleted";
                                $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                            }

                        }).error(function (result) {

                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.ShowNew = true;
                $scope.Holiday = item;
            }
        }

        $scope.AddNew = function () {
            $scope.Holiday = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ShowNew = true;
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "HolidayService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }
    }]);