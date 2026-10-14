
var app = angular.module('InfinityLab');

app.controller('DeliveryController', ['UserDetails', '$scope', '$http', '$location',
    '$rootScope', '$window', 'restService', '$filter', '$timeout', 'ngDialog', 'localStorageService', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout, ngDialog, localStorageService, $stateParams) {
        $scope.TotalBillValue = 0;
        $scope.SearchRef = restService.SearchReference;
        $scope.locationid = null;
        $scope.counterid = null;
        $scope.billprefix = null;
        $scope.LastReceived = null;
        $scope.allowcancel = false;
        $scope.candeliver = false;
        $scope.delivered = false;
        $scope.enablescan = false;
        $scope.totalgiftamount = 0;
        $scope.totalgiftqty = 0;
        $scope.totalrecivedqty = 0;
        $scope.totaldeliveryqty = 0;
        $scope.totalcouponvalue = 0;
        $scope.ReadyForDelivery = false;
        $scope.ReadyForDeliveryByBillNo = false;
        $scope.supportdeliverybarcode = false;
        $scope.supportdeliverybillno = false;
        $scope.delivery = {};
        $scope.bills = [];
        $scope.billitems = [];
        $scope.giftitems = [];
        $scope.ErrorMessage = 'Status....';
        $scope.Deliverydate = new Date();
        $scope.autoCoding = { 'code': '' };
        $scope.IsGiftCoupon = $stateParams.type == 'GIFTCOUPONISSUE';
        $scope.IsGiftDelivery = $stateParams.type == 'GIFTDELIVERY';
        $scope.IsGiftDeliveryIssue = $stateParams.type == 'GIFTDELIVERYISSUE';
        $scope.IsDelivery = $stateParams.type == 'DELIVERY';
        $scope.IsAlteration = $stateParams.type == 'ALTERATION';
        $scope.StockMarkers = [];
        $scope.ReversedStockMarkers = [];
        $scope.StockMarkersids = [];
        $scope.GiftStockMarkers = [];
        $scope.HasDiscountVoucher = false;
        $scope.GiftVoucherMarkers = [];
        $scope.GiftVoucherMarkerids = [];
        $scope.GiftVoucherValue = 0;
        $scope.GiftDiscountVoucherMarkers = [];
        if ($scope.IsGiftCoupon) {
            restService.Get('ReferencelistService/FULL-MARKER', true).then(function (results) {

                $.each(results, function (k, v) {
                    if (v.additionalinfo &&
                        v.additionalinfo.giftconfig &&
                        v.additionalinfo.giftconfig.isgiftcoupon) {
                        var gitem = {
                            'id': v.id, 'name': v.name, 'value': v.additionalinfo.giftconfig.billvalue,
                            'discountvalue': v.additionalinfo.giftconfig.giftcouponvalue,
                            'discountpercentage': v.additionalinfo.giftconfig.giftcouponpercentage,
                            'expiry': v.additionalinfo.giftconfig.giftcouponexpiry,
                            'validfrom': v.additionalinfo.giftconfig.giftcouponfrom,
                            'fromdays': v.additionalinfo.giftconfig.giftcouponvalidafterdays, 
                            'expirydays': v.additionalinfo.giftconfig.giftcouponexpirydays,
                        };
                        $scope.GiftDiscountVoucherMarkers.push(gitem);
                    }
                })

                $scope.GiftDiscountVoucherMarkers.sort(function (a, b) {
                    return a.value < b.value ? -1 : a.value > b.value ? 1 : 0;
                });

                for (var i = 0; i < $scope.GiftDiscountVoucherMarkers.length; i++) {
                    var v = $scope.GiftDiscountVoucherMarkers[i];
                    v.fvalue = v.value;
                    if ($scope.GiftDiscountVoucherMarkers.length > i + 1)
                        v.tvalue = $scope.GiftDiscountVoucherMarkers[i + 1].value;
                    else
                        v.tvalue = 10000000;
                }
            });
        }

        if ($scope.IsGiftDelivery || $scope.IsGiftDeliveryIssue) {
            restService.Get('ReferencelistService/FULL-MARKER', true).then(function (results) {

                $.each(results, function (k, v) {
                    if (v.additionalinfo.issales) {
                        var gitem = {
                            'id': v.id, 'name': v.name, 'value': v.additionalinfo.billdiscount,
                            'discountvoucher': v.additionalinfo.maxlimit
                        };
                        //startValue = v.additionalinfo.billdiscount;
                        if (v.additionalinfo.maxlimit > 0) {
                            $scope.HasDiscountVoucher = true;
                            $scope.GiftVoucherMarkers.push(gitem);
                        }
                        else if (v.additionalinfo.billdiscount > 0) {
                            $scope.StockMarkers.push(gitem);
                            $scope.StockMarkersids.push(v.id);
                        }
                        else {
                            $scope.GiftVoucherMarkers.push(gitem);
                            $scope.StockMarkersids.push(v.id);
                        }
                    }
                })

                $scope.StockMarkers.sort(function (a, b) {
                    return a.value < b.value ? -1 : a.value > b.value ? 1 : 0;
                });

                for (var i = 0; i < $scope.StockMarkers.length; i++) {
                    var v = $scope.StockMarkers[i];
                    v.fvalue = v.value;
                    if ($scope.StockMarkers.length > i + 1)
                        v.tvalue = $scope.StockMarkers[i + 1].value;
                    else
                        v.tvalue = 10000000;
                }
                //var startValue = 0;
                //var toValue = 0
                //$.each($scope.StockMarkers, function (k, v) {
                //    if (k == 0) {
                //        toValue = v.value;

                //    }
                //    v.fvalue = startValue;
                //    v.tvalue = v.value;
                //    startValue = v.value;
                //});

                $scope.ReversedStockMarkers = (JSON.parse(JSON.stringify($scope.StockMarkers)));

                $scope.ReversedStockMarkers.sort(function (a, b) {
                    return a.value > b.value ? -1 : a.value < b.value ? 1 : 0;
                });

            });
        }

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            $scope.counterid = $rootScope.LocationInfo.counter.id;
            $scope.CounterName = $scope.SearchRef($rootScope.LocationInfo.counter.id, results);

        });

        $scope.TailoringCharges = restService.Get('ReferencelistService/FULL-TAILORINGCHARGE', true).then(function (results) {
            $scope.TailoringCharges = results;
            $.each(results, function (k, v) {
                v.measurement = [];
                if (v.additionalinfo != 'undefined' && v.additionalinfo != null &&
                    v.additionalinfo.mfgconfig != 'undefined' && v.additionalinfo.mfgconfig != null &&
                    v.additionalinfo.mfgconfig.measurement != 'undefined' && v.additionalinfo.mfgconfig.measurement != null) {
                    v.charge = v.additionalinfo.mfgconfig.charge;
                    v.taxid = v.additionalinfo.mfgconfig.taxid;
                    $.each(v.additionalinfo.mfgconfig.measurement.split(','), function (kk, vv) {
                        v.measurement.push({
                            'key': vv, 'value': '', 'focusonenter': 'mfe_' + k, 'focusid': 'mf_' + k, 'islast': false
                        });
                    });
                    if (v.measurement.length > 0)
                        v.measurement[v.measurement.length - 1].islast = true;
                }
            });
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
            $scope.locationid = $rootScope.LocationInfo.location.id;
            $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, results);
        });

        $scope.ApplicationSettings = function (locationinfo) {

            var srequest = JSON.stringify({
                "Item": { 'locationid': locationinfo.location.id, 'key': 'APPLICATIONSETTINGS' },
                "securitytoken": "String content"
            });

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.supportdeliverybarcode = false || result[0].appsettings.system_supportdeliverybarcode;
                    $scope.supportdeliverybillno = false || result[0].appsettings.system_supportdeliverybillno;
                    $scope.system_giftdeliveryhighest = false || result[0].appsettings.system_giftdeliveryhighest;
                    $scope.system_giftissuereceive = false || result[0].appsettings.system_giftissuereceive;

                    if (result[0].appsettings.system_barcodeparser != 'undefined' &&
                        result[0].appsettings.system_barcodeparser != null &&
                        result[0].appsettings.system_barcodeparser != '') {
                        $scope.BarcodeParser = JSON.parse(result[0].appsettings.system_barcodeparser);
                        if ($scope.BarcodeParser != null) {
                            $scope.BarcodeParser.IsEmpty = false;
                        }
                        else {
                            $scope.BarcodeParser = { 'IsEmpty': true };
                        }
                    }
                    else {
                        $scope.BarcodeParser = { 'IsEmpty': true };
                    }
                    $scope.BarcodeParser.UpdateBarcode = function (barcode) {
                        if (this.IsEmpty) return barcode;
                        if (this.exclude != '' && barcode.startsWith(this.exclude)) return barcode;
                        if (this.length > 0) return barcode.substr(this.start, this.length);
                        if (this.removeend > 0) return barcode.substr(0, barcode.length - this.removeend);
                        if (this.removebegin > 0) return barcode.substr(this.removebegin);
                        return barcode;
                    }
                    $scope.allowcancel = $rootScope.UserInfo.p_SALES.modify;
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("Please save the application setting for the current location");
            });

            $scope.Counters = restService.Get('DeliveryService/GetReceived/' + $rootScope.UserInfo.OrganizationID + '/' + $scope.IsDelivery + '/' +
                locationinfo.location.id + '/' + locationinfo.counter.id, false).then(function (results) {
                    $scope.TotalBillValue = results;
                });
        }

        $rootScope.OnLocationInfoReady(function (locationinfo) {
            $scope.ApplicationSettings(locationinfo);
        });

        $scope.EmployeeList = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-', 'code': '-' });
            }
            $scope.EmployeeList = results;
        });


        $scope.ShowReturnDialog = function (item) {
            var dialog = ngDialog.open({
                template: 'Return2Emloyee',
                data: {
                    'EmployeeList': $scope.EmployeeList, 'item': item
                },
                width: '450px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.EmployeeList = $scope.ngDialogData.EmployeeList;
                    $scope.returnto = 0;
                    $scope.Selected = function () {
                        $scope.ngDialogData.returnto = $scope.returnto;
                        $scope.closeThisDialog($scope.ngDialogData);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {

                if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                    data.value.item.returnto = data.value.returnto;
                    $scope.ChangeStatus(3, data.value.item);
                }
            });
        }

        $scope.GetNumber = function (value) {

            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };

        $scope.CheckLocation = function () {

            var printerData = localStorageService.get('LocationData');
            if (printerData != null) {
                $scope.locationid = printerData.location.id;
                $scope.counterid = printerData.counter.id;
            }
            if ($scope.locationid == null) {
                $rootScope.ShowAlert('No-Location', 'Please select the location');
                return false;
            }
            return true;
        }


        $scope.CanDeliver = function () {
            var barcode = $scope.billitems.find(function (item) {
                return typeof (item.rqty) == 'undefined' || item.rqty < item.piece;
            });
            return typeof (barcode) == 'undefined' || barcode == null;
        }

        $scope.SaveGiftDelivery = function () {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == undefined || printerData == null) {
                $rootScope.ShowAlert('Printer','Please select the printer');
                return;
            }
            if ($scope.IsGiftDelivery && $scope.TotalBillValue - $scope.totalgiftamount < 0) {
                $rootScope.Toaster.Info('Gift Value Cant exceed');
                return;
            }

            var giftlist = [];
            if ($scope.system_giftissuereceive && $scope.IsGiftDelivery) {
                $.each($scope.StockMarkers, function (k, v) {
                    if (v.giftcount && v.giftcount > 0) {
                        giftlist.push({ 'stockmarkerid': v.id, 'qty': v.giftcount, 'value': v.giftvalue, 'issued': false });
                    }
                })

                $.each($scope.GiftVoucherMarkers, function (k, v) {
                    if (v.giftcount && v.giftcount > 0) {
                        giftlist.push({ 'stockmarkerid': v.id, 'qty': v.giftcount, 'value': v.giftvalue, 'issued': false });
                    }
                })

                if (giftlist.length == 0) {
                    $rootScope.Toaster.Info('These bill/settlement dont have any gift');
                    return;
                }
            }

            if ($scope.IsGiftDeliveryIssue) {
                var irrqty = null;

                $.each($scope.StockMarkers, function (k, v) {
                    if (v.qty != v.iqty) {
                        irrqty = v;
                    }
                })

                $.each($scope.GiftVoucherMarkers, function (k, v) {
                    if (v.qty != v.iqty) {
                        irrqty = v;
                    }
                })

                if (irrqty != null) {
                    $rootScope.Toaster.Error('Gift Qty and Issued Qty Missmatch - ' + irrqty.name);
                    return;
                }
            }

            var request = {
                "Item": {
                    'locationid': $scope.locationid,
                    'counterid': $scope.counterid,
                    'bills': $scope.bills,
                    'giftitems': $scope.giftitems,
                    'giftlist': giftlist,
                    'status': 2,
                    'voucherinfo': $scope.HasDiscountVoucher ? { 'amount': $scope.GiftVoucherValue } : null
                },
                "SecurityToken": "String content"
            };
            if ($scope.IsGiftDeliveryIssue) {
                request.Item = $scope.bills[0];
                request.Item.status = 2;
                $.each($scope.StockMarkers, function (k, v) {
                    if (v.iqty > 0) {
                        var giftitem = request.Item.giftlist.find(function (item) {
                            return item.stockmarkerid == v.id;
                        });
                        if (giftitem && giftitem != null) {
                            giftitem.issued = true;
                            giftitem.salablegoodsid = v.salablegoodsid;
                            giftitem.barcode = v.barcode;
                        }
                    }
                });

                $.each($scope.GiftVoucherMarkers, function (k, v) {
                    if (v.iqty > 0) {
                        var giftitem = request.Item.giftlist.find(function (item) {
                            return item.stockmarkerid == v.id;
                        });
                        if (giftitem && giftitem != null) {
                            giftitem.issued = true;
                            giftitem.salablegoodsid = v.salablegoodsid;
                            giftitem.barcode = v.barcode;
                        }
                    }
                });
            }

            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/SaveGiftDelivery",
                data: srequest
            }).success(function (result) {
                $scope.candeliver = false;
                $scope.bills = [];
                $scope.GiftCount = 0;
                $.each($scope.StockMarkers, function (k, v) {
                    v.giftcount = 0;
                    v.giftvalue = 0;
                    v.qty = 0;
                    v.iqty = 0;
                    v.barcode = '';
                });
                $.each($scope.GiftVoucherMarkers, function (k, v) {
                    v.giftcount = 0;
                    v.giftvalue = 0;
                    v.qty = 0;
                    v.iqty = 0;
                    v.barcode = '';
                });
                $scope.GiftValue = 0;
                $scope.GiftRemain = 0;
                $scope.GiftVoucherValue = 0;
                $scope.billitems = [];
                $scope.totalgiftqty = 0;
                $scope.totalgiftamount = 0;
                $scope.totalrecivedqty = 0;
                $scope.totaldeliveryqty = 0;
                $scope.totalcouponvalue = 0;
                $scope.autoCoding.code = '';
                $scope.TotalBillValue = 0;
                $scope.giftitems = [];
                $scope.autoCoding.code = '';
                $scope.ErrorMessage = '';
                $scope.delivery = result;

               if (result.printbarcode && result.printbarcode != '')
                    $rootScope.Sent2Printer(result.printbarcode);
               else if (result.printdocument && result.printdocument != '')
                    $rootScope.Sent2Printer(result.printdocument);

            });
        }


        $scope.SaveGiftCopon = function () {

            if ($scope.IsGiftCoupon && $scope.TotalBillValue - $scope.totalcouponvalue < 0) {
                $rootScope.Toaster.Info('Gift Coupon Cant exceed');
                return;
            }

            if ($scope.totalcouponvalue <= 0) {
                $rootScope.Toaster.Info('Gift Coupon not available for this bill value');
                return;
            }

            var printerData = localStorageService.get('PrinterData');
            if (printerData == undefined || printerData == null) {
                $rootScope.ShowAlert('Printer','Please select the printer');
                return;
            }

            var request = {
                "Item": {
                    'locationid': $scope.locationid,
                    'counterid': $scope.counterid,
                    'bills': $scope.bills,
                    'status': 2,
                    'voucherinfo': {
                        'amount': $scope.totalcouponvalue,
                        'expiry': $scope.totalcouponexpiry,
                        'expirydays': $scope.totalcouponexpirydays,
                        'fromdays': $scope.totalcouponfromdays,
                        'validfrom': $scope.totalcouponvalidfrom
                    }
                },
                "SecurityToken": "String content"
            };

            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/SaveGiftCoupon",
                data: srequest
            }).success(function (result) {
                $scope.candeliver = false;
                $scope.bills = [];
                $scope.GiftCount = 0;
                $scope.GiftValue = 0;
                $scope.GiftRemain = 0;
                $scope.GiftVoucherValue = 0;
                $scope.billitems = [];
                $scope.totalgiftqty = 0;
                $scope.totalgiftamount = 0;
                $scope.totalrecivedqty = 0;
                $scope.totaldeliveryqty = 0;
                $scope.totalcouponvalue = 0;
                $scope.totalcouponexpiry = '';
                $scope.totalcouponexpirydays = '';
                $scope.totalcouponvalidfrom = '';
                $scope.totalcouponfromdays = 0;
                $scope.autoCoding.code = '';
                $scope.TotalBillValue = 0;
                $scope.giftitems = [];
                $scope.autoCoding.code = '';
                $scope.ErrorMessage = '';
                $scope.delivery = result;
                if (result.printbarcode && result.printbarcode != '')
                    $rootScope.Sent2Printer(result.printbarcode);
                else if (result.printdocument && result.printdocument != '')
                    $rootScope.Sent2Printer(result.printdocument);

            });
        }

        $scope.DeliveryBill = function (code) {
            var bill = $scope.bills.find(function (item) {
                return item.billno == code;
            });
            if (bill && bill != null) {

                if (bill.candeliver) {

                    var items = [];
                    items.push(bill);
                    var request = {
                        "Items": (JSON.parse(JSON.stringify(items))),
                        "SecurityToken": "String content"
                    };

                    $.each(request.Items, function (k, v) {
                        v.status = 2;
                        v.isalteration = $scope.IsAlteration;
                    });

                    var srequest = JSON.stringify(request);
                    $scope.SaveRequest = true;
                    $http({
                        method: 'POST',
                        url: "DeliveryService/UpdateAll",
                        data: srequest
                    }).success(function (result) {

                        if (bill.billinfo != null && bill.billinfo.items != null) {
                            $.each(bill.billinfo.items, function (kk, vv) {
                                var itemindex = $scope.billitems.indexOf(vv);
                                if (itemindex >= 0) {
                                    $scope.billitems.splice(itemindex, 1);
                                }
                            });
                        }

                        bill.deliverystatus = 'Delivered';
                        bill.style = {
                            "background-color": "black",
                            "color": "white"
                        };

                        var _items = $scope.bills.find(function (item) {
                            return item.deliverystatus == 'Validated';
                        });

                        if (_items == 'undefined' || _items == null)
                            $scope.bills = [];
                        //$scope.bills.splice($scope.bills.indexOf(bill), 1);
                        if ($scope.bills.length == 0) {
                            $scope.ReadyForDeliveryByBillNo = false;
                            $scope.candeliver = false;
                            $scope.bills = [];
                            $scope.billitems = [];
                            $scope.totalgiftqty = 0;
                            $scope.totalgiftamount = 0;
                            $scope.totalrecivedqty = 0;
                            $scope.totaldeliveryqty = 0;
                            $scope.totalcouponvalue = 0;
                            $scope.autoCoding.code = '';
                            $scope.TotalBillValue = 0;
                            $scope.giftitems = [];
                            $scope.GiftCount = 0;
                            $.each($scope.StockMarkers, function (k, v) {
                                v.giftcount = 0;
                                v.giftvalue = 0;
                            });
                            $scope.GiftValue = 0;
                            $scope.GiftRemain = 0;
                            $scope.GiftVoucherValue = 0;
                        }
                    });
                    return true;
                }
                //if ($scope.candeliver && ($scope.supportdeliverybarcode || $scope.ReadyForDelivery)) {
                //    $scope.ChangeStatus(2);
                //}
            }
            return false;
        }

        $scope.SearchAutoCoding = function () {
            if ($scope.autoCoding.code == null || $scope.autoCoding.code == "") {
                return;
            }

            if ($scope.autoCoding.code == '*') {
                $scope.ReadyForDeliveryByBillNo = false;
                $scope.candeliver = false;
                $scope.bills = [];
                $scope.billitems = [];
                $scope.totalgiftqty = 0;
                $scope.totalgiftamount = 0;
                $scope.totalrecivedqty = 0;
                $scope.totaldeliveryqty = 0;
                $scope.totalcouponvalue = 0;
                $scope.autoCoding.code = '';
                $scope.TotalBillValue = 0;
                $scope.giftitems = [];
                $scope.GiftCount = 0;
                $.each($scope.StockMarkers, function (k, v) {
                    v.giftcount = 0;
                    v.giftvalue = 0;
                });
                $scope.GiftValue = 0;
                $scope.GiftRemain = 0;
                $scope.GiftVoucherValue = 0;
                return;
            }

            if ($scope.autoCoding.code == '0' && $scope.IsGiftCoupon) {
                $scope.SaveGiftCopon();
                return;
            }

            if ($scope.autoCoding.code == '0' && ($scope.IsGiftDelivery || $scope.IsGiftDeliveryIssue)) {
                $scope.SaveGiftDelivery();
                return;
            }


            if ($scope.IsDelivery && $scope.bills && $scope.bills != null && $scope.bills.length > 0) {
                if ($scope.autoCoding.code == 'return') {
                    $scope.ShowReturnDialog($scope.bills[0]);
                    return;
                }


                if ($scope.ReadyForDeliveryByBillNo) {
                    if ($scope.DeliveryBill($scope.autoCoding.code)) {
                        $scope.autoCoding.code = '';
                    }
                    return;
                }
                else if ($scope.autoCoding.code == '0') {
                    if ($scope.enablescan) {
                        if ($scope.ReadyForDelivery) {
                            $scope.ChangeStatus(1);
                        }
                        else {
                            $rootScope.Toaster.Error("Please scan all the billed products barcode!");
                        }
                    }
                    else if ($scope.candeliver && ($scope.supportdeliverybarcode || $scope.ReadyForDelivery)) {
                        $scope.ChangeStatus(2);
                    }
                    else {
                        $rootScope.Toaster.Error("Can't deliver");
                    }
                    return;
                }


                var barcode = $scope.billitems.find(function (item) {
                    return item.barcode == $scope.autoCoding.code && item.rqty < item.piece;
                });

                if (typeof (barcode) == 'undefined' || barcode == null) {
                    if (!$scope.BarcodeParser.IsEmpty) {
                        var ubarcode = $scope.BarcodeParser.UpdateBarcode($scope.autoCoding.code);

                        var barcode = $scope.billitems.find(function (item) {
                            return item.barcode == ubarcode && item.rqty < item.piece;
                        });
                    }
                }

                if (barcode && barcode != null) {
                    if (typeof (barcode.rqty) == 'undefined' || barcode.rqty == 0) {
                        barcode.rqty = 1;
                    }
                    else {
                        barcode.rqty += 1;
                    }
                    $scope.totaldeliveryqty++;
                    if (barcode.piece == barcode.rqty) {
                        barcode.style = {
                            "background-color": "black",
                            "color": "white"
                        };
                    }
                    else {
                        barcode.style = {};
                    }
                    $scope.billitems.splice($scope.billitems.indexOf(barcode), 1);
                    $scope.billitems.splice(0, 0, barcode);
                    $scope.ReadyForDelivery = $scope.CanDeliver();
                    $scope.autoCoding.code = '';
                    return;
                }
            }

            var isbillrequest = false;
            var request = {
                "Item": {
                    'searchbill': true,
                    'isalteration': $scope.IsAlteration,
                    'isgift': $scope.IsGiftDelivery,
                    'stockmarkersids': $scope.StockMarkersids,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'counterid': $rootScope.LocationInfo.counter.id,
                    'settlementcode': $scope.autoCoding.code,
                    'isgiftissue': $scope.IsGiftDeliveryIssue,
                    'isgiftcoupon': $scope.IsGiftCoupon
                },
                "SecurityToken": "String content"
            };


            if ($scope.billprefix != null) {
                if ($scope.autoCoding.code.startsWith($scope.billprefix)) {
                    request = {
                        "Item": {
                            'searchbill': true,
                            'locationid': $rootScope.LocationInfo.location.id,
                            'settlementcode': $scope.autoCoding.code,
                            'counterid': $rootScope.LocationInfo.counter.id,
                            'isalteration': $scope.IsAlteration,
                            'isgift': $scope.IsGiftDelivery,
                            'isgiftissue': $scope.IsGiftDeliveryIssue,
                            'isgiftcoupon': $scope.IsGiftCoupon
                        },
                        "SecurityToken": "String content"
                    };
                    isbillrequest = true;
                }
            }

            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ReadyForDeliveryByBillNo = false;
                if (result && result.Success == false && result.ErrorMessage != null && result.ErrorMessage != '') {
                    if ($scope.IsGiftDelivery && result.ErrorMessage == "GIFTREPRINT") {
                        $rootScope.Toaster.Error("Gift Voucher Re-Print Enabled.");
                        $scope.delivery = result.Items[0];
                        return;
                    }
                    else if ($scope.IsGiftCoupon && result.ErrorMessage == "GIFTCOUPONREPRINT") {
                        $rootScope.Toaster.Error("Gift Coupon Re-Print Enabled.");
                        $scope.delivery = result.Items[0];
                        return;
                    }
                    else {
                        $rootScope.Toaster.Error(result.ErrorMessage);
                        $scope.ErrorMessage = result.ErrorMessage;
                        $scope.autoCoding.code = '';
                        return;
                    }
                }

                if (result && result.Success) {
                    var oldautoCoding = $scope.autoCoding.code;
                    $scope.autoCoding.code = '';
                    if ($scope.IsAlteration) {
                        if (result.Items[0].id > 0) {
                            $scope.delivery = result.Items[0];
                            $scope.delivery.customername = result.Items[0].customername;
                            $scope.delivery.contactno = result.Items[0].contactno;
                            $scope.delivery.customerid = result.Items[0].customerid;
                            $scope.enablescan = $scope.delivery.status == 0;
                            $scope.candeliver = $scope.delivery.status >= 0 && $scope.delivery.status < 2;
                            $scope.delivered = $scope.delivery.status == 2;

                            $scope.bills = [];
                            $scope.billitems = [];

                            $.each($scope.delivery.billinfo.items, function (k, v) {
                                var bill = $scope.bills.find(function (item) {
                                    return item.billno = v.billno;
                                });
                                if (typeof (bill) == 'undefined' || bill == null) {
                                    bill = {
                                        'billno': v.billno, 'qty': 0, 'amount': 0, 'rqty': 0,
                                        'settlementcode': v.settlementcode
                                    }
                                    $scope.bills.push(bill);
                                }
                                bill.qty += v.piece;
                                bill.rqty += v.rqty;
                                bill.amount += v.chargeamount;
                                $scope.billitems.push(v)
                            });
                        }
                        else {
                            $scope.enablescan = false;
                            $scope.candeliver = false;
                            $scope.delivered = false;
                            $scope.delivery.customername = result.Items[0].customername;
                            $scope.delivery.contactno = result.Items[0].contactno;
                            $scope.delivery.customerid = result.Items[0].customerid;
                            $scope.totalrecivedqty = 0;
                            $scope.totaldeliveryqty = 0;
                            $scope.totalcouponvalue = 0;
                            $.each(result.Items, function (k, v) {
                                $scope.bills.push(v);
                                if (v.billinfo != null && v.billinfo.items != null) {
                                    $.each(v.billinfo.items, function (kk, vv) {
                                        if (vv.piece > 0) {
                                            vv.billno = v.billno;
                                            vv.rqty = v.candeliver ? vv.piece : 0;
                                            $scope.totalrecivedqty += vv.piece;
                                            vv.selected = false;
                                            $scope.billitems.push(vv)
                                        }
                                    });
                                }
                            });
                        }
                        $scope.autoCoding.code = '';
                    }
                    else if ($scope.IsDelivery) {
                        if ($scope.totalrecivedqty > 0 && (result.ErrorMessage == 'NOT PAID' || result.ErrorMessage == 'BILL CANCELLED')) {
                            $scope.autoCoding.code = oldautoCoding;
                            return;
                        }
                        $scope.enablescan = false;
                        $scope.candeliver = false;
                        $scope.delivered = false;
                        if (result.Items.length > 0 && result.Items[0].received) {
                            $scope.LastReceived = result.Items[0];
                            $scope.delivery = result.Items[0];
                            $scope.TotalBillValue = $scope.LastReceived.receivedcount;
                            $scope.autoCoding.code = '';
                            if (!$scope.supportdeliverybarcode) {
                                if (result.ErrorMessage && result.ErrorMessage != null && result.ErrorMessage != '')
                                    $rootScope.Toaster.Notice(result.ErrorMessage);
                                return;
                            }
                            else {
                                $scope.enablescan = true;
                            }
                        }
                        var billitems = [];
                        var bills = [];
                        $scope.totalrecivedqty = 0;
                        $scope.totaldeliveryqty = 0;
                        $scope.totalcouponvalue = 0;
                        $.each(result.Items, function (k, v) {

                            bills.push(v);
                            if (v.billinfo != null && v.billinfo.items != null) {
                                $.each(v.billinfo.items, function (kk, vv) {
                                    if (vv.piece > 0) {
                                        vv.rqty = v.candeliver ? vv.piece : 0;
                                        $scope.totalrecivedqty += vv.piece;
                                        billitems.push(vv)
                                    }
                                });
                            }
                        });
                        $scope.candeliver = result.Items[0].candeliver;
                        $scope.delivered = result.Items[0].status == 2;
                        $scope.bills = bills;
                        $scope.billitems = billitems;
                        $scope.autoCoding.code = '';
                        $scope.ErrorMessage = result.ErrorMessage;
                        if (result.ErrorMessage == 'NOT PAID') {
                            $rootScope.ShowAlert('NOT PAID', 'Given bill doesnt have settlement');
                        } else if (result.ErrorMessage == 'BILL CANCELLED') {
                            $rootScope.ShowAlert('BILL CANCELLED', 'Given bill may be cancelled or not found');
                        }
                        else if (!$scope.enablescan && !$scope.candeliver && !$scope.delivered) {
                            $scope.totalrecivedqty = 0;
                            $scope.totaldeliveryqty = 0;
                            $scope.totalcouponvalue = 0;
                            $scope.candeliver = false;
                            $scope.delivered = false;
                            $scope.bills = [];
                            $scope.billitems = [];
                            $scope.autoCoding.code = '';
                            if (result.Items.length > 0)
                                $rootScope.ShowAlert('Status', 'No items received at delivery counter for this bill no/settlement no');
                            else
                                $rootScope.ShowAlert('Status', 'Its not deliverable goods');
                        }
                        else if (result.ErrorMessage != null && result.ErrorMessage != '') {
                            $rootScope.Toaster.Info(result.ErrorMessage);
                        }
                        else if ($scope.supportdeliverybillno) {
                            if ($scope.supportdeliverybarcode && $scope.enablescan) {
                                $rootScope.Toaster.Info('Scan barcodes');
                            }
                            else {
                                $scope.candeliver = false;
                                if (result.Items[0].settlementcode == oldautoCoding) {
                                    $scope.ReadyForDeliveryByBillNo = true;
                                }
                                else {
                                    $scope.totalrecivedqty = 0;
                                    $scope.totaldeliveryqty = 0;
                                    $scope.totalcouponvalue = 0;
                                    $scope.candeliver = false;
                                    $scope.delivered = false;
                                    $scope.bills = [];
                                    $scope.billitems = [];
                                    $scope.autoCoding.code = '';
                                    if (result.Items[0].billno == oldautoCoding) {
                                        if (result.Items[0].status == 1) {
                                            $rootScope.ShowAlert('Ready', 'Already kept in rack, its ready for delivery');
                                        }
                                    }
                                    else {
                                        $rootScope.ShowAlert('Invalid', 'Please Scan Settlement Code instead of Bill No');
                                    }
                                }
                            }
                        }
                        $scope.ReadyForDelivery = false;
                    }
                    else if ($scope.IsGiftDeliveryIssue) {
                        if (result.status == 6 && result.barcode && result.barcode != '') {
                            if ($scope.bills && $scope.bills.length > 0) {
                                var stockmarker = $scope.StockMarkers.find(function (item) {
                                    return item.id == result.stockmarkerid;
                                });
                                if (stockmarker && stockmarker != null) {
                                    stockmarker.iqty += 1;
                                    stockmarker.salablegoodsid = result.salablegoodsid;
                                    stockmarker.barcode = result.barcode;
                                } else {
                                    var stockmarker = $scope.GiftVoucherMarkers.find(function (item) {
                                        return item.id == result.stockmarkerid;
                                    });
                                    if (stockmarker && stockmarker != null) {
                                        stockmarker.iqty += 1;
                                        stockmarker.salablegoodsid = result.salablegoodsid;
                                        stockmarker.barcode = result.barcode;
                                    }
                                }
                            }
                            else {
                                $rootScope.Toaster.Error('Please scan gift voucher');
                                return;
                            }
                        }
                        else {
                            $scope.delivery.customername = result.Items[0].customername;
                            $scope.delivery.contactno = result.Items[0].contactno;
                            $scope.bills = result.Items;

                            $.each($scope.StockMarkers, function (k, v) {
                                v.qty = 0;
                                v.iqty = 0;
                                var gitem = result.Items[0].giftlist.find(function (item) {
                                    return item.stockmarkerid == v.id;
                                });
                                if (gitem && gitem != null) {
                                    v.qty += gitem.qty;
                                }
                            });

                            $.each($scope.GiftVoucherMarkers, function (k, v) {
                                v.qty = 0;
                                v.iqty = 0;
                                var gitem = result.Items[0].giftlist.find(function (item) {
                                    return item.stockmarkerid == v.id;
                                });
                                if (gitem && gitem != null) {
                                    v.qty += gitem.qty;
                                }
                            });

                        }
                    }


                    else if ($scope.IsGiftCoupon) {

                        $scope.delivery.customername = result.Items[0].customername;
                        $scope.delivery.contactno = result.Items[0].contactno;
                        if ($scope.bills == undefined || $scope.bills == null)
                            $scope.bills = [];

                        if ($scope.TotalBillValue == undefined || $scope.TotalBillValue == null)
                            $scope.TotalBillValue = 0;

                        $.each(result.Items, function (k, v) {
                            var exist = $scope.bills.find(function (item) {
                                return item.billno == v.billno;
                            });
                            if (exist == 'undefined' || exist == null) {
                                $scope.bills.push(v);
                                $scope.TotalBillValue += v.amount;
                            }
                            else
                                $rootScope.Toaster.Error('Already Added : ' + exist.billno);
                        })

                        $scope.ComputeGiftCoupon();
                    }

                    else {
                        if (result.status == 6) {
                            var exist = $scope.giftitems.find(function (item) {
                                return item.barcode == result.barcode;
                            });
                            if (exist && exist != null) {
                                if ($scope.totalgiftamount + exist.value > $scope.TotalBillValue) {
                                    $rootScope.Toaster.Error('Gift Value Cant exceed');
                                    return;
                                }
                                exist.qty++;
                                exist.nqty = exist.qty;
                                $scope.totalgiftqty++;
                                $scope.totalgiftamount += exist.value;
                            }
                            else {
                                var stockmarker = $scope.StockMarkers.find(function (item) {
                                    return item.id == result.stockmarkerid;
                                });
                                if (stockmarker) {
                                    if ($scope.totalgiftamount + stockmarker.value > $scope.TotalBillValue) {
                                        $rootScope.Toaster.Error('Gift Value Cant exceed');
                                        return;
                                    }
                                    $scope.giftitems.push({
                                        'stockmarkerid': stockmarker.stockmarkerid, 'name': result.name, 'marker': stockmarker.name,
                                        'value': stockmarker.value, 'qty': 1, 'barcode': result.barcode, 'stockid': result.stockid,
                                        'salablegoodsid': result.salablegoodsid, 'locationid': result.locationid, 'nqty': 1,
                                        'productid': result.productid, 'salestaxid': result.salestaxid
                                    });
                                    $scope.totalgiftamount += stockmarker.value;
                                    $scope.totalgiftqty++;
                                }
                            }
                        }
                        else {

                            $scope.delivery.customername = result.Items[0].customername;
                            $scope.delivery.contactno = result.Items[0].contactno;
                            $.each(result.Items, function (k, v) {
                                var exist = $scope.bills.find(function (item) { return item.billid == v.billid; });
                                if (typeof (exist) == 'undefined') {
                                    $scope.bills.push(v);
                                    $scope.TotalBillValue += v.amount;
                                }
                            });
                            $scope.ComputeGiftDelivery();
                        }
                    }

                }

            });
        }

        $scope.GiftQtyChanged = function (item) {
            var qty = parseInt(item.nqty);
            if (!isNaN(qty)) {
                var oqty = item.qty;
                item.qty = item.nqty;
                $scope.totalgiftamount = 0;
                $scope.totalgiftqty = 0;
                $.each($scope.giftitems, function (k, v) {
                    $scope.totalgiftamount += (v.value * v.qty);
                    $scope.totalgiftqty += v.qty;
                });
                if ($scope.TotalBillValue - $scope.totalgiftamount < 0) {
                    $rootScope.Toaster.Info('Gift Value Cant exceed');
                    item.qty = oqty;
                    item.nqty = oqty;
                    $scope.totalgiftamount = 0;
                    $scope.totalgiftqty = 0;
                    $.each($scope.giftitems, function (k, v) {
                        $scope.totalgiftamount += (v.value * v.qty);
                        $scope.totalgiftqty += v.qty;
                    });
                }
                $scope.ComputeGiftDelivery();
            }
        }

        $scope.DeleteGiftItem = function (item) {
            $scope.giftitems.splice($scope.giftitems.indexOf(item), 1);
            $scope.totalgiftamount -= (item.value * item.qty);
            $scope.totalgiftqty = item.qty;
            $scope.ComputeGiftDelivery();
        }


        $scope.DeleteBill = function (item) {
            if ($scope.IsGiftDelivery) {
                var settlementcode = item.settlementcode;
                var ditem = [];
                $.each($scope.bills, function (k, v) {
                    if (v.settlementcode == settlementcode) {
                        ditem.push(v);
                    }
                });
                $.each(ditem, function (k, v) {
                    $scope.bills.splice($scope.bills.indexOf(v), 1);
                    $scope.TotalBillValue -= v.amount;
                })
            }
            else {
                $scope.bills.splice($scope.bills.indexOf(item), 1);
                $scope.TotalBillValue -= item.amount;
            }

            if ($scope.TotalBillValue - $scope.totalgiftamount < 0) {
                $.each($scope.giftitems, function (k, v) {
                    if ($scope.TotalBillValue - $scope.totalgiftamount < 0) {
                        $scope.totalgiftamount -= (v.value * v.qty);
                        $scope.totalgiftqty -= v.qty;
                        v.qty = 0;
                        v.nqty = 0;
                    }
                });
            }
            $scope.ComputeGiftDelivery();
        }


        $scope.ComputeGiftCoupon = function () {
            $scope.totalcouponvalue = 0;
            $.each($scope.GiftDiscountVoucherMarkers, function (k, v) {
                if (v.fvalue <= $scope.TotalBillValue &&
                    v.tvalue >= $scope.TotalBillValue) {
                    $scope.totalcouponexpiry = v.expiry;
                    $scope.totalcouponexpirydays = v.expirydays;
                    $scope.totalcouponvalidfrom = v.validfrom;
                    $scope.totalcouponfromdays = v.fromdays;
                    if (v.discountpercentage &&
                        v.discountpercentage > 0) {
                        $scope.totalcouponvalue = Math.round($scope.TotalBillValue * v.discountpercentage / 100, 0);
                    }
                    else {
                        $scope.totalcouponvalue = v.discountvalue;
                    }
                }
            });
        }

        $scope.IsGiftAuto = false;
        $scope.ComputeGiftDelivery = function () {
            var available = $scope.TotalBillValue;
            $scope.GiftCount = 0;
            $scope.GiftValue = 0;
            $scope.GiftRemain = 0;
            $scope.GiftVoucherValue = 0;
            if ($scope.HasDiscountVoucher) {
                if ($scope.GiftVoucherMarkers.length == 1) {
                    if ($scope.GiftVoucherMarkers[0].value < $scope.TotalBillValue) {
                        var sm = $scope.GiftVoucherMarkers[0];
                        if (sm && sm != null) {
                            $scope.GiftVoucherValue = $scope.TotalBillValue * sm.discountvoucher / 100;
                        }
                    }
                }
                else if ($scope.GiftVoucherMarkers[0].value < $scope.TotalBillValue) {
                    var sm = $scope.GiftVoucherMarkers.find(function (item) {
                        return item.value > $scope.TotalBillValue;
                    });
                    if (typeof (sm) == 'undefined' || sm == null) {
                        sm = $scope.GiftVoucherMarkers[$scope.GiftVoucherMarkers.length - 1];
                    }
                    else if ($scope.GiftVoucherMarkers[0].value < sm.value) {
                        sm = $scope.GiftVoucherMarkers[$scope.GiftVoucherMarkers.indexOf(sm) - 1];
                    }
                    if (sm && sm != null) {
                        $scope.GiftVoucherValue = $scope.TotalBillValue * sm.discountvoucher / 100;
                    }
                }

            }
            else {
                var StockList = [];
                $scope.GiftValue = 0;
                $scope.GiftCount = 0;
                $.each($scope.StockMarkers, function (k, v) {
                    v.giftcount = 0;
                    v.giftvalue = 0;
                });
                $.each($scope.ReversedStockMarkers, function (k, v) {
                    v.giftcount = 0;
                    v.giftvalue = 0;

                    if ($scope.system_giftdeliveryhighest && $scope.GiftCount > 0)
                        return;
                    if (v.value <= available) {
                        var nextvalue = available % v.value;
                        v.giftcount = (available - nextvalue) / v.value;
                        v.giftvalue = v.giftcount * v.value;
                        available = nextvalue;
                        $scope.GiftValue += v.giftvalue;
                        $scope.GiftCount += v.giftcount;
                        StockList.push({
                            'qty': v.giftcount, 'markerid': v.id
                        });
                    }
                    var aitem = $scope.StockMarkers.find(function (item) {
                        return item.id == v.id;
                    });
                    aitem.giftcount = v.giftcount;
                    aitem.giftvalue = v.giftvalue;
                });
                $scope.GiftRemain = available;

                $.each($scope.GiftVoucherMarkers, function (k, v) {
                    v.giftcount = 0;
                    v.giftvalue = 0;
                    $.each($scope.bills, function (kk, vv) {
                        $.each(vv.billinfo.items, function (ik, iv) {
                            if (iv.freegift &&
                                iv.giftmarkerid == v.id) {
                                v.giftcount += iv.qty;
                                v.giftvalue += iv.amount;
                            }
                        });
                    });
                });

                if ($scope.IsGiftAuto) {

                    var request = {
                        "Item": {
                            'locationid': $rootScope.LocationInfo.location.id,
                            'StockList': StockList
                        },
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $scope.SaveRequest = true;
                    $http({
                        method: 'POST',
                        url: "StockmarkerService/SearchStock",
                        data: srequest
                    }).success(function (result) {
                        $scope.totalgiftamount = 0;
                        $scope.totalgiftqty = 0;
                        $scope.giftitems = [];
                        if (result && result != null && result.length > 0) {
                            $.each(result, function (k, v) {
                                var stockmarker = $scope.StockMarkers.find(function (item) {
                                    return item.id == v.markerid;
                                });
                                $scope.giftitems.push({
                                    'stockmarkerid': v.markerid, 'name': v.productname, 'marker': stockmarker.name,
                                    'value': stockmarker.value, 'qty': 1, 'barcode': v.barcode, 'stockid': v.stockid,
                                    'salablegoodsid': v.salablegoodsid, 'locationid': v.locationid
                                });
                                $scope.totalgiftamount += stockmarker.value;
                                $scope.totalgiftqty++;
                            });
                        }
                    });
                }
            }
        }

        $scope.UpateAlteration = function (status) {
            $.each($scope.billitems, function (k, v) {
                if (v.selected) {
                    v.status = status;
                }
            });

            $scope.delivery.status = status;
            var request = {
                "Item": $scope.delivery,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/UpateAlteration",
                data: srequest
            }).success(function (result) {
                $scope.ReadyForDelivery = false;
                $scope.candeliver = false;
                $scope.enablescan = false;
                $scope.delivered = false;
                $scope.totalrecivedqty = 0;
                $scope.totaldeliveryqty = 0;
                $scope.totalcouponvalue = 0;
                $scope.totalalerationqty = 0;
                $scope.TotalBillValue = 0;
                $scope.delivery = {};
                $scope.bills = [];
                $scope.billitems = [];
                $scope.autoCoding.code = '';
                $scope.ErrorMessage = '';
            });
        }


        $scope.DeliveryCountChanged = function () {
            $scope.totalalerationqty = 0;
            $scope.totalalerationcharge = 0;
            $.each($scope.billitems, function (k, v) {
                if (v.selected) {
                    v.chargeamount = v.rqty * v.chargerate;
                    $scope.totalalerationqty += v.rqty;
                    $scope.totalalerationcharge += v.rqty * v.chargerate;
                }
            })
        }


        $scope.AlterationCountChanged = function () {
            $scope.totalalerationqty = 0;
            $scope.totalalerationcharge = 0;
            $.each($scope.bills, function (k, v) {
                v.rqty = 0;
            });
            $.each($scope.billitems, function (k, v) {
                if (v.selected) {
                    v.chargeamount = 0;
                    var qty = parseInt(v.rqty);
                    if (!isNaN(qty)) {
                        if (v.piece < qty) {
                            qty = v.piece;
                            v.rqty = qty;
                        }
                        v.chargeamount = qty * v.chargerate;
                        $scope.totalalerationqty += qty;
                        $scope.totalalerationcharge += qty * v.chargerate;
                        $scope.bills.find(function (item) {
                            return item.billno == v.billno
                        }).rqty += qty;
                    }
                }
            })
        }

        $scope.UpdateMeasureConfig = function (item) {
            item.chargerate = 0;
            var tailoring = $scope.TailoringCharges.find(function (sitem) {
                return sitem.id == item.tailoringid;
            });
            if (tailoring && tailoring != null) {
                item.chargerate = tailoring.charge;
                item.chargename = tailoring.name;
            }
            $scope.AlterationCountChanged();
        }

        $scope.SaveAlteration = function () {

            if (typeof ($scope.delivery.customername) == 'undefined' || $scope.delivery.customername == null || $scope.delivery.customername == '') {
                $rootScope.Toaster.Error("Please enter the customer name");
                return;
            }

            if (typeof ($scope.delivery.contactno) == 'undefined' || $scope.delivery.contactno == null || $scope.delivery.contactno == '') {
                $rootScope.Toaster.Error("Please enter the customer contact no");
                return;
            }

            if (typeof ($scope.delivery.validatedby) == 'undefined' || $scope.delivery.validatedby == null || $scope.delivery.validatedby <= 0) {
                $rootScope.Toaster.Error("Please select the tailor");
                return;
            }

            var billinfo = {
                'items': [], 'chargeamount': 0, 'qty': 0, 'amount': 0, 'billno': ''
            };

            $.each($scope.billitems, function (k, v) {
                if (v.selected && v.rqty && v.rqty > 0) {
                    billinfo.items.push(v);
                    v.chargeamount = v.chargerate * v.rqty;
                    billinfo.amount += v.rqty * v.price;
                    billinfo.qty += v.rqty;
                    billinfo.chargeamount += v.chargeamount;
                    if (billinfo.billno.indexOf(v.billno) < 0) {
                        billinfo.billno += (billinfo.billno != '' ? ',' : '') + v.billno;
                    }
                }
            })

            $scope.delivery.billinfo = billinfo;
            $scope.delivery.isalteration = true;
            $scope.delivery.locationid = $scope.locationid;
            $scope.delivery.counterid = $scope.counterid;
            $scope.delivery.isactive = true;
            $scope.delivery.version = 1;

            var request = {
                "Item": $scope.delivery,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/SaveAlteration",
                data: srequest
            }).success(function (result) {

                $scope.ReadyForDelivery = false;
                $scope.candeliver = false;
                $scope.enablescan = false;
                $scope.delivered = false;
                $scope.totalrecivedqty = 0;
                $scope.totaldeliveryqty = 0;
                $scope.totalcouponvalue = 0;
                $scope.delivery = {};
                $scope.bills = [];
                $scope.billitems = [];
                $scope.autoCoding.code = '';
                $scope.ErrorMessage = '';

                if (result.printdocument && result.printdocument != '')
                    $rootScope.Sent2Printer(result.printdocument);
            });
        }

        $scope.Print = function () {
            var request = {
                "Item": $scope.delivery,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/Print",
                data: srequest
            }).success(function (result) {
                if (result.printdocument && result.printdocument != '')
                    $rootScope.Sent2Printer(result.printdocument);
            });
        }

        $scope.PrintGiftVoucher = function () {
            $scope.delivery.isalteration = false;
            var request = {
                "Item": $scope.delivery,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/Print",
                data: srequest
            }).success(function (result) {
                if (result.printdocument && result.printdocument != '')
                    $rootScope.Sent2Printer(result.printdocument);
            });
        }


        $scope.ChangeStatus = function (status, item) {

            if (status == 2) {
                if (!$scope.candeliver && ($scope.supportdeliverybarcode || $scope.ReadyForDelivery)) {
                    return;
                }
            }
            var items = [];

            if (typeof (item) != 'undefined') {
                items.push(item);
            }
            else {
                items = $scope.bills;
            }

            items = (JSON.parse(JSON.stringify(items)));
            var request = {
                "Items": (JSON.parse(JSON.stringify(items))),
                "SecurityToken": "String content"
            };

            $.each(request.Items, function (k, v) {
                v.status = status;
                v.isalteration = $scope.IsAlteration;
            });

            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "DeliveryService/UpdateAll",
                data: srequest
            }).success(function (result) {
                if (result.status == 2) {
                    $scope.LastDelivery = result;
                }
                //var billitems = [];
                //var bills = [];
                //$.each(result.Items, function (k, v) {

                //    bills.push(v);
                //    if (v.billinfo != null && v.billinfo.items != null) {
                //        $.each(v.billinfo.items, function (kk, vv) {
                //            billitems.push(vv)
                //        });
                //    }
                //});
                $scope.ReadyForDelivery = false;
                $scope.candeliver = false;
                $scope.enablescan = false;
                $scope.delivered = false;
                $scope.totalrecivedqty = 0;
                $scope.totaldeliveryqty = 0;
                $scope.totalcouponvalue = 0;
                $scope.delivery = {};
                $scope.bills = [];
                $scope.billitems = [];
                $scope.autoCoding.code = '';
                $scope.ErrorMessage = '';
            });

        };


        $timeout(function () {

            $scope.setFocusautocoding();
            $scope.CheckLocation();
            if (typeof ($rootScope.LocationInfo.billconfig) != 'undefined' &&
                $rootScope.LocationInfo.billconfig != null) {
                $scope.billprefix = $rootScope.LocationInfo.billconfig.prefix;
                if (typeof ($scope.billprefix) == 'undefined' || $scope.billprefix == '')
                    $scope.billprefix = null;
            }

        }, 10);
    }]);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
