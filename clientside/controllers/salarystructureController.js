
var app = angular.module('InfinityLab');

app.controller('salarystructureController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Entity = {
            "id": 0,
            "name": "",
            "fromdate": "",
            "todate": "",
            "mode": "",
            "fixedbasic": "",
            "fixedda": "",
            "deducttextsalaryid": "",
            "splallowanceid":"",
            "configuration": [],
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": ""
        };

        $scope.refpaycategorylist = restService.Get('ReferencelistService/FULL-PAYCATEGORY', true).then(function (results) {
            $scope.refpaycategorylist = results;

            $.each(results, function (k, v) {
                $scope.Entity.configuration.push({
                    'id': v.id, 'code': v.code, 'formula':
                        v.additionalinfo == null ? '' : v.additionalinfo.formula
                });
            });
            $scope.SalaryStructure = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.firstLength = parseInt($scope.SalaryStructure.configuration.length / 2)+1;
        });

        $scope.Replacecodechanged = function () {
            var replaceitem = $scope.SalaryStructure.configuration.find(function (item) {
                return item.id == $scope.SalaryStructure.replaceid;
            });
            if (typeof (replaceitem) != 'undefined') {
                $scope.SalaryStructure.sourceformula = replaceitem.formula;
                $scope.SalaryStructure.newformula = replaceitem.formula;
            }
        }

        $scope.SearchRef = restService.SearchReference;
        $scope.ShowNew = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowNew = !$scope.ShowNew;
        }
        $scope.ErrorStyle = "success";
      

        $scope.AddNew = function () {
            $scope.SalaryStructure = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ShowNew = true;
        }




        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.Save = function () {

            if ($scope.SalaryStructure.fromdate == null || $scope.SalaryStructure.fromdate == 0) {
                $scope['setFocusfromdate']();
                $rootScope.Toaster.Error("Please select fromdate");
                return;
            }
            if ($scope.SalaryStructure.todate == null || $scope.SalaryStructure.todate == 0) {
                $scope['setFocustodate']();
                $rootScope.Toaster.Error("Please select todate");
                return;
            }

            request = {
                "Item": $scope.SalaryStructure,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalarystructureService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Salary Structure Saved Successfully.!");
                    $scope.SalaryStructure = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }

        $scope.ReplaceFormula = function () {
            var request = {
                "Item": {
                    'id': $scope.SalaryStructure.id, 'headerid': $scope.SalaryStructure.replaceid, 'source': $scope.SalaryStructure.sourceformula, 'replace': $scope.SalaryStructure.newformula
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalarystructureService/Replace",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    var replaceitem = $scope.SalaryStructure.configuration.find(function (item) {
                        return item.id == $scope.SalaryStructure.replaceid;
                    });
                    if (typeof (replaceitem) != 'undefined') {
                        replaceitem.formula = $scope.SalaryStructure.newformula;
                    }

                    $rootScope.Toaster.Info("Salary Structure Saved Successfully.!");
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }

        $scope.ManageSearchItem = function (action, item) {
            if ($rootScope.UserInfo.p_SALARYSTRUCTURE.modify || $rootScope.UserInfo.p_SALARYSTRUCTURE.delete) {
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
                                        url: "EmployeesalarystructureService/Delete",
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

                    if (item.configuration == null) {
                        item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));
                    }
                    else {
                        $.each($scope.Entity.configuration, function (k, v) {
                            var itm = item.configuration.find(function (vv) {
                                return (v.code == vv.code);
                            });
                            if (itm == null)
                                item.configuration.push({
                                    'id': v.id, 'code': v.code, 'formula': v.formula
                                });
                        });
                    }

                    $scope.SalaryStructure = item;


                    $scope.ShowNew = true;
                }
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalarystructureService/Search",
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