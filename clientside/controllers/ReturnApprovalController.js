
var app = angular.module('InfinityLab');

app.controller('ReturnApprovalController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
function (UserDetails, $scope, $http, $location, $rootScope, $window, restService) {


    $scope.ShowNew = false;
    $scope.ShowPending = false;
    $scope.SearchResult = [];

    $scope.AddNew = function () {
        $scope.Academic = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ShowNew = true;
    }

    $scope.Pending = function () {
        $scope.Academic = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ShowPending = true;
    }

    $scope.AddItem = function () {
        $scope.ApprovalItems.push($scope.Employee.accident);
        $scope.Employee = (JSON.parse(JSON.stringify($scope.Entity)));
    }

   


    $scope.Entity = {
        "code": "",
        "name": "",
        "type": 0,
        "issalestax": false,
        "ispurchasetax": false,
        "isactive": true,
        "createdby": 0,
        "modifiedby": 0,
        "version": 0,
        "id": 0,
        "createdon": "0001-01-01T00:00:00",
        "modifiedon": "0001-01-01T00:00:00",
        "disable": false,
        "taxlist": ""
    };


    $scope.ReturnApproval = (JSON.parse(JSON.stringify($scope.Entity)));

    $scope.SwitchPanel = function () {
        $scope.ShowNew = !$scope.ShowNew;
    }

    $scope.ErrorMessage = "";



    $scope.SearchCondition = {
        code: "",
        name: ""
    }

    $scope.Clear = function () {
        $scope.Tax = (JSON.parse(JSON.stringify($scope.Entity)));
    }

    $scope.Save = function () {

        request = {
            "Item": $scope.Academic,
            "securitytoken": "String content"
        }

        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "AcadamicService/Save",
            data: srequest
        }).success(function (result) {

            if (result != null && result.id > 0) {
                $scope.ErrorMessage = "Successfully saved";
                $scope.ErrorStyle = "success";
                $scope.Academic = (JSON.parse(JSON.stringify($scope.Entity)));
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
                        url: "TaxService/Delete",
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
            $scope.ShowSearch = false;
            $scope.Tax = item;
            $.each($scope.TaxList, function (k, v) {
                v.selected = false;
                var id = '' + v.id;
                if ($scope.Tax.taxlist != null && $scope.Tax.taxlist.length > 0) {
                    $.each($scope.Tax.taxlist.split(','), function (kk, vv) {
                        if (v.id == vv) {
                            v.selected = true;
                        }
                    });
                }
            });
        }
    }

    $scope.PreLoadTaxList = function () {
        request = {
            "Item": {
                code: "",
                name: ""
            },
            "securitytoken": "String content"
        }

        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "TaxService/Search",
            data: srequest
        }).success(function (result) {
            $.each(result, function (k, v) {
                $scope.TaxList.push(v);
            });
        }).error(function (result) {
        });
    }();

    $scope.Search = function () {

        request = {
            "Item": $scope.SearchCondition,
            "securitytoken": "String content"
        }

        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "TaxService/Search",
            data: srequest
        }).success(function (result) {

            $scope.SearchResult = result;
        }).error(function (result) {
        });

    }
}]);
