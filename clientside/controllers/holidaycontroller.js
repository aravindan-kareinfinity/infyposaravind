
var app = angular.module('InfinityLab');

app.controller('holidaycontroller', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {


        $scope.hoildaytypelist = [{ 'id': 'National Holiday', 'name': 'National Holiday' },
            { 'id': 'Yearly Holiday', 'name': 'Yearly Holiday' },   
            { 'id': 'Local Holiday', 'name': 'Local Holiday' },
            { 'id': 'Festival', 'name': 'Festival' }];

        $scope.SearchRef = restService.SearchReference;
        $scope.ShowNew = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowNew = !$scope.ShowNew;
        }

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


        $scope.Holiday = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.SearchCondition = {
            name: "",
            description: "",
            holidayon: "",
            holidaytype: "",
            institutionid: 0,
            academicid: 0
        }

        $scope.Save = function () {

            if ($scope.Holiday.name == null || $scope.Holiday.name == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please fill out holoidayname");
                return;
            }
            if ($scope.Holiday.holidaytype == null || $scope.Holiday.holidaytype == 0) {
                $scope['setFocusholidaytype']();
                $rootScope.Toaster.Error("Please select holidaytype ");
                return;
            }
          
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

        $scope.CloneLeaves = function () {
            var ids = [];
            $.each(SearchResult, function (k, v) {
                if (v.selected)
                    ids.push(v.id);
            });

            if (ids.length == 0) return;

            $rootScope.Prompt('Copy to Year', 'Year', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {
                    var srequest = { 'Year': code, 'ids': ids };
                    $http({
                        method: 'POST',
                        url: "HolidayService/Clone",
                        data: srequest
                    }).success(function (result) {
                        $rootScope.Toaster.Info("Successfully Cloned days - "+result);
                    }).error(function (result) {
                    });
                }
            })
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
                    buttons: {
                        formSubmit: {
                            text: 'Submit',
                            btnClass: 'btn-blue',
                            action: function () {
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
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
	jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
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
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);