
var app = angular.module('InfinityLab');

app.controller('DailyExpencesController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {

    // $scope.TaxTypes = [{ 'ID': 1, 'Name': 'Billable Item' }, { 'ID': 2, 'Name': 'Stockable Item' }, { 'ID': 3, 'Name': 'Refundable Item' }, { 'ID': 4, 'Name': 'Receivable Item' }];

    $scope.TaxTypes = [{ 'id': 1, 'name': 'Sales Tax' }, { 'id': 2, 'name': 'Purchase Tax' },
        { 'id': 3, 'name': 'Service Tax' }, { 'id': 4, 'name': 'Value Added Tax' }, { 'id': 5, 'name': 'GST' },
        { 'id': 6, 'name': 'Surcharge' }, { 'id': 8, 'name': 'Custom Duty' }, { 'id': 9, 'name': 'Excise Duty' },
        { 'id': 10, 'name': 'Education Tax' }, { 'id': 11, 'name': 'Swach Bharat Cess' }, { 'id': 12, 'name': 'Krishi Kalyan Cess' }];

    $scope.SearchRef = restService.SearchReference;

    $scope.TaxList = [{
        selected: true,
        code: "bprice",
        id: 0,
        isactive: true,
        ispurchasetax: false,
        issalestax: false,
        name: "Base Price",
        taxpercentage: 100,
        type: -1,
        version: 0
    }];

    //$scope.ShowSearch = false;
    //$scope.SearchResult = [];

    //$scope.SwitchPanel = function () {
    //    $scope.ShowSearch = !$scope.ShowSearch;
    //}

    //$scope.ErrorMessage = "";

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


    $scope.DailyExpences = (JSON.parse(JSON.stringify($scope.Entity)));

    $scope.ErrorStyle = "success";
    $scope.ShowSearch = false;
    $scope.SearchResult = [];

    //$scope.PreLoadTaxList();

    $scope.SwitchPanel = function () {
        $scope.ShowSearch = !$scope.ShowSearch;
        $scope.Tax = (JSON.parse(JSON.stringify($scope.Entity)));
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
            "Item": $scope.Tax,
            "securitytoken": "String content"
        }

        $scope.Tax.taxlist = "";
        $.each($scope.TaxList, function (k, v) {
            if (v.selected) {
                $scope.Tax.taxlist += ($scope.Tax.taxlist.length > 0 ? "," : "") + v.id;
            }
        });

        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "TaxService/Save",
            data: srequest
        }).success(function (result) {

            if (result != null && result.id > 0) {
                $scope.ErrorMessage = "Successfully saved";
                $scope.ErrorStyle = "success";
                var exist = false;
                $.each($scope.TaxList, function (key, value) {

                    if (!exist && value.id == result.id) {
                        exist = true;
                    }
                });

                if (!exist) {
                    $scope.TaxList.push(result);
                }

                $scope.Tax = (JSON.parse(JSON.stringify($scope.Entity)));
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
    $timeout(function () {
        $scope.setFocusfirst();
    }, 10);
}]);
