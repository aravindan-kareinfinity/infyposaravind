
var app = angular.module('InfinityLab');

app.controller('TaxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        // $scope.TaxTypes = [{ 'ID': 1, 'Name': 'Billable Item' }, { 'ID': 2, 'Name': 'Stockable Item' }, { 'ID': 3, 'Name': 'Refundable Item' }, { 'ID': 4, 'Name': 'Receivable Item' }];

        $scope.TaxTypes = [{ 'id': 1, 'name': 'GST' }, { 'id': 2, 'name': 'Ranged GST' },
            { 'id': 3, 'name': 'VAT' }, { 'id': 4, 'name': 'TDS' }, { 'id': 5, 'name': 'TCS' },
            { 'id': 6, 'name': 'CESS' }];

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
            "issalestax": true,
            "ispurchasetax": true,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "id": 0,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "disable": false,
            "taxlist": "",
            "taxsplit": { 'items': [] }
        };


        $scope.Tax = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.TaxItem = { 'fromamount': 0, 'toamount': 0, 'taxpercentage':0, 'cgst': 0, 'igst': 0, 'sgst': 0 };

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        //$scope.PreLoadTaxList();

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Tax = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.AddTaxItem = function () {
            if (typeof ($scope.Tax.taxsplit.items) == 'undefined' || $scope.Tax.taxsplit.items == null)
                $scope.Tax.taxsplit.items = [];
            $scope.Tax.taxsplit.items.push($scope.TaxItem);
            $scope.TaxItem = { 'fromamount': 0, 'toamount': 0, 'taxpercentage':0, 'cgst': 0, 'igst': 0, 'sgst': 0 };
        }

        $scope.taxpercentagechanged = function (percentage,taxitem) {
            var tp = parseFloat(percentage);
            if (isNaN(tp)) tp = 0;
            if (tp == 0) {
                taxitem.sgst = taxitem.igst = taxitem.cgst = 0;
            }
            else {
                taxitem.sgst = taxitem.igst = taxitem.cgst = tp / 2;
            }
        }

        $scope.RemoveTaxItem = function (item) {
            $scope.Tax.taxsplit.items.splice($scope.Tax.taxsplit.items.indexOf(item), 1);
        }

        $scope.SearchCondition = {
            code: "",
            name: ""
        }

        $scope.New = function () {
            $scope.Tax = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Save = function () {

            if ($scope.Tax.code == null || $scope.Tax.code == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please fill out taxcode");
                return;
            }
            if ($scope.Tax.name == null || $scope.Tax.name == 0) {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please fill out tax name");
                return;
            }

            if ($scope.Tax.type == 1) {
                $scope.Tax.taxsplit = { 'items': [] };
                $scope.Tax.taxsplit.items.push($scope.TaxItem);
            } else if ($scope.Tax.type > 2) {
                var tp = parseFloat($scope.Tax.taxpercentage);
                if (isNaN(tp)) tp = 0;
                if(tp>0)
                $scope.TaxItem = {
                    'fromamount': 0, 'toamount': 0, 'taxpercentage': tp,
                    'cgst': $scope.Tax.taxpercentage / 2, 'igst': tp / 2, 'sgst': tp / 2
                    };
                else
                    $scope.TaxItem = {
                        'fromamount': 0, 'toamount': 0, 'taxpercentage': tp,
                        'cgst':0, 'igst':0, 'sgst': 0
                    };
                $scope.Tax.taxsplit = { 'items': [] };
            }

            request = {
                "Item": $scope.Tax,
                "securitytoken": "String content"
            }


            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TaxService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.Tax = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

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
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.Tax = item;
                if (item.type != 2 && item.taxsplit.items.length > 0) {
                    $scope.TaxItem = {
                        'fromamount': 0, 'toamount': 0,
                        'cgst': item.taxsplit.items[0].cgst,
                        'igst': item.taxsplit.items[0].igst,
                        'sgst': item.taxsplit.items[0].sgst
                    };
                }
                /*$.each($scope.TaxList, function (k, v) {
                    v.selected = false;
                    var id = '' + v.id;
                    if ($scope.Tax.taxlist != null && $scope.Tax.taxlist.length > 0) {
                        $.each($scope.Tax.taxlist.split(','), function (kk, vv) {
                            if (v.id == vv) {
                                v.selected = true;
                            }
                        });
                    }
                });*/

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
