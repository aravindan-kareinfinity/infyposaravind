
var app = angular.module('InfinityLab');

app.controller('FestivalDiscountController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.DiscountModes = [{ 'id': 1, 'name': 'By Discount Price' }, { 'id': 2, 'name': 'By Discount %' } , ];

        $scope.Status = [{ 'id': 0, 'name': 'Yet to Apply' }, { 'id': 1, 'name': 'Applied' }];

        $scope.ItemList = [];

        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });

        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });

        restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo == null || v.additionalinfo.iswarehouse == false)
                    $scope.Locations.push(v);
            });
        });


        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });

        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "schemedate": null,
            "schemeappliedon": null,
            "schemeremovedon": null,
            "schemeappliedby": 0,
            "reason": null,
            "companyid": 0,
            "locationid": 0,
            "filter": { 'productid': 0, 'materialid': 0, 'brandid': 0, 'patternid': 0, 'sizeid': 0, 'styelid': 0, 'colourid': 0 },
            "discountmode": 2,
            "discountvalue": 0.0,
            "rounding": 0.0,
            "status": 0,
        };

        $scope.SearchCondition = {
            "supplierid": 0,
            "productid": 0,
            "brandid": 0,
            "patternid": 0,
            "materialid": 0,
            "sizeid": 0,
            "styleid": 0,
            "colourid": 0,
        };

        $scope.FestivalDiscount = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.FestivalDiscount.schemedate = new Date();

        //$scope.PreloadResult = [{
        //    'id': 1, 'discountdate': '21-02-2017',
        //    'discountapplyby': 'Raja', 'reason': 'dummy', 'fixedrate': '1000', 'fixeddiscount': '5%',
        //    'roundingnear': 'kk', 'supplierid': 1, 'productid': 6
        //}];

        $scope.LoadResults = [];

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Festival = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";




        $scope.Save = function () {
            if ($scope.FestivalDiscount.id > 0 && $scope.FestivalDiscount.status == 1)
            {
                $rootScope.Toaster.Error("Edit not allowed once the discount applied.");
                return;
            }
            var request = {
                "Item": {
                    'Scheme': $scope.FestivalDiscount,
                    'Items': $scope.ItemList
                 },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountschemeService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.Scheme != null && result.Scheme.id > 0) {
                    $rootScope.Toaster.Info("Discount Plan Saved Successfully.!");

                    $scope.FestivalDiscount.id = result.Scheme.id;
                    //$scope.FestivalDiscount = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete Item") {
                if (item.status == 0)
                    $scope.ItemList.splice($scope.ItemList.indexOf(item), 1);
                else
                    item.status = 2;
            }
            else  if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.reason + "?",
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
                                    url: "DiscountschemeService/Delete",
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
                $scope.ShowSearch = false;
                $scope.FestivalDiscount = item;
                $scope.SearchItems(false, item.id);
                $scope.FestivalDiscount.discountmode = parseInt(item.discountmode);
            }
        }

        $scope.SearchByBarcode = function (barcode) {
            $scope.FestivalDiscount.filter.barcode = barcode;
            $scope.FestivalDiscount.filter.locationid = $scope.FestivalDiscount.locationid;
            $scope.SearchItems(true, 0);
        };

        $scope.SearchItems = function (isSearch, discountschemeid) {
            if ($scope.FestivalDiscount.locationid == null || $scope.FestivalDiscount.locationid == 0)
            {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            if (!isSearch) {
                $scope.FestivalDiscount.filter.discountschemeid = discountschemeid;
                $scope.FestivalDiscount.companyid = 0;
            }
            else {
                $scope.FestivalDiscount.companyid = $scope.FestivalDiscount.companyid;
                $scope.FestivalDiscount.filter.discountschemeid = 0;
            }
            var request = {
                "Item": $scope.FestivalDiscount.filter,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountschemeService/SearchItems",
                data: srequest
            }).success(function (result) {
                if (!isSearch || $scope.ItemList.length==0) {
                    $scope.ItemList = result;
                }
                else {
                    $.each(result, function (k, v) {
                        var exist = $scope.ItemList.find(function (itm) {
                            return (itm.stockid == v.stockid);
                        })
                        if (exist == null) {
                            $scope.ItemList.push(v);
                        }
                    });
                }
            }).error(function (result) {
            });
        }

        $scope.PrintBarcode = function () {
            var ids = [];
            $.each($scope.ItemList, function (k, v) {
                if (v.selected)
                    ids.push(v.stockid);
            });

            $scope.ChoseReport( { 'stockids': ids, 'reportid':0 });
        };

        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "StockService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {

                $rootScope.Sent2Printer(v, printerData);
                //var request = {
                //    'id': printerData.printerid,
                //    'content': v,
                //    'encoded': 'BASE64'
                //};
                //var srequest = JSON.stringify(request);
                //$http({
                //    method: 'POST',
                //    url: printerData.service,
                //    data: srequest
                //}).success(function (result) {

                //}).error(function (error) {

                //});
            })
        }

        $scope.ShowBarcodeList = function (requestitem, BarcodeList) {
            var dialog = ngDialog.open({
                template: 'ChooseBarecodes',
                data: {
                    'requestitem': requestitem,
                    'BarcodeList': BarcodeList
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.BarcodeList = $scope.ngDialogData.BarcodeList
                    $scope.requestitem = $scope.ngDialogData.requestitem;

                    $scope.Selected = function () {
                        var blist = [];
                        $.each($scope.ngDialogData.BarcodeList, function (k, v) {
                            if (v.selected)
                                blist.push(v.code);
                        });
                        $scope.requestitem.filterbarcode = blist.join(',');
                        $scope.closeThisDialog($scope.requestitem);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ChoseReport(data.value);
                    }
                }
            });
        }

        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "StockService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {

                $rootScope.Sent2Printer(v, printerData);
                //var request = {
                //    'id': printerData.printerid,
                //    'content': v,
                //    'encoded': 'BASE64'
                //};
                //var srequest = JSON.stringify(request);
                //$http({
                //    method: 'POST',
                //    url: printerData.service,
                //    data: srequest
                //}).success(function (result) {

                //}).error(function (error) {

                //});
            })
        }

        $scope.ChangeDiscount = function (status) {
            request = {
                "Item": {
                    id: $scope.FestivalDiscount.id,
                    status: status
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountschemeService/ChangeDiscount",
                data: srequest
            }).success(function (result) {
                $scope.FestivalDiscount = result;
            }).error(function (result) {
            });
        };

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountschemeService/Search",
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
