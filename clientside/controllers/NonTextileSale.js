var app = angular.module('InfinityLab');
app.controller('NonTextileSaleController', ['UserDetails', '$scope', '$http', '$location', '$rootScope',
    '$window', 'restService', '$filter', '$timeout', 'localStorageService', '$interval', 'hotkeys', '$stateParams', 'ngDialog', '$sce', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout, localStorageService,
        $interval, hotkeys, $stateParams, ngDialog, $sce, cntxService) {
        $scope.holdList = [];
        $scope.LastBill = null;
        $scope.ShowAll = true;
        $scope.fcategory = '';
        $scope.query = '';
        $scope.customerPhone = '';
        $scope.billItems = [];
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
            $scope.floorname = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.floorid, results) : 'Select Floor';
        });

        $scope.Locations = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = results;

            if (!$scope.HasLocationSelected) {
                $scope.LocationName = 'Select Location';
                return;
            }
            $scope.ShowLocationInfo();
        });

        

        $scope.Hold = function (item) {
            if (item == undefined) {
                if ($scope.billItems.length > 0) {
                    $scope.holdList.push({ 'did': $scope.billItems.length, 'items': $scope.billItems });
                    $scope.billItems = [];
                }
            }
            else {
                $scope.holdList.splice($scope.holdList.indexOf(item), 1);
                $.each($scope.holdList, function (k, v) {
                    v.did = v.items.length;
                });
                $scope.billItems = item.items;
            }
            $scope.CalculateBillValue();
        }

        $scope.AllItems = [];
        $scope.OutOfStockItems = [];
        $scope.ShowLocationInfo = function () {
            $scope.CounterName = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters) : 'Select Counter';
            $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
            $.each($scope.Locations, function (k, v) {
                if (v.id == $rootScope.LocationInfo.location.id) {
                    var additionalinfo = v.additionalinfo;
                    if (additionalinfo != null)
                        $scope.IsWarehouse = additionalinfo.iswarehouse;
                    if ($scope.IsWarehouse)
                        $rootScope.Toaster.Error("Sales will not be allowed in Warehouse, Please check the sales location.");
                }
            });


            restService.Get('ReferencelistService/FULL-SWIPPINGMACINE', true).then(function (results) {
                if (typeof ($rootScope.LocationInfo.location.id) != 'undefined') {
                    $scope.SwippingList = [];
                    $.each(results, function (k, v) {
                        if (typeof (v.additionalinfo) != 'undefined' &&
                            typeof (v.additionalinfo.locationid) != 'undefined') {
                            if (v.additionalinfo.locationid == $rootScope.LocationInfo.location.id)
                                $scope.SwippingList.push(v);
                        }
                        else {
                            $scope.SwippingList.push(v);
                        }
                    });
                }
                else {
                    $scope.SwippingList = results;
                }
            });

            restService.Get('ReferencelistService/TOUCHITEMS/' + $rootScope.LocationInfo.location.id, true).then(function (results) {
                $scope.TouchItems = results;
                var allproductgroup = { 'productgroup': 'ALL', 'productgroupid': 0, products: [] };
                $.each(results, function (k, v) {
                    $.each(v.products, function (kk, vv) {
                        if (vv.imageid && vv.imageid != '') {
                            vv.hasimage = true;
                            vv.image = 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + vv.imageid;
                        }
                        vv.productgroup = v.productgroup;
                        vv.productgroupid = v.productgroupid;
                        if (v.foodcounter == undefined)
                            vv.foodcounter = 'CONFIGURE FC';
                        else
                            vv.foodcounter = v.foodcounter;
                        allproductgroup.products.push(vv);
                    });
                });
                $scope.TouchItems.splice(0, 0, allproductgroup);
                $scope.AllItems = allproductgroup.products;
                $timeout(function () {
                    angular.element($('.prodcategory')[0]).addClass('active');
                    $scope.Products = allproductgroup.products
                    $scope.ProductGroup = allproductgroup;
                    $scope.RefreshOutOfStock();
                }, 100);
            });
        }

        $scope.SortProducts = function () {
            $.each($scope.TouchItems, function (k, v) {
                v.products.sort(function (first, next) {
                    if (first.productname < next.productname) {
                        return -1;
                    }
                    if (first.productname > next.productname) {
                        return 1;
                    }
                    return 0;
                });
            })
        }

        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.AllProducts = null;
        restService.Get('ReferencelistService/ALLPRODUCTS', true).then(function (results) {
            $scope.AllProducts = results;
        });

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            if ($scope.HasLocationSelected && !$scope.AutoSettlement) {
                var counter = results.find(function (item) { return item.id == $rootScope.LocationInfo.counter.id; });
                $scope.AutoSettlement = typeof (counter) != 'undefined' && counter.autosettlement;
                $scope.OnlineCounter = typeof (counter) != 'undefined' && counter.online;
                if ($scope.AutoSettlement) {
                    $rootScope.Toaster.Notice("Single window settlement enabled..");
                    $scope.ValidateCounter();
                }
            }
        });

        $scope.ShowOutOfStock = false;
        $scope.CurrentOutOfStock = null;
        $scope.OutOfStock = function (enable) {
            $scope.ShowOutOfStock = enable;
            if ($scope.CurrentOutOfStock) {
                $scope.RefreshOutOfStock();
            }
        }

        $scope.SaveOutOfStock = function () {
            var listofids = [];
            $.each($scope.TouchItems[0].products, function (k, v) {
                if (v.outofstock)
                    listofids.push(v.productid);
            });

            if ($scope.TouchItems[$scope.TouchItems.length - 1].productgroupid == -1) {
                $.each($scope.TouchItems[$scope.TouchItems.length - 1].products, function (k, v) {
                    if (v.outofstock)
                        listofids.push(v.productid);
                });
            }

            var request = {
                "Item": {
                    'type': 'OutOfStock', 'isactive': true,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'companyid': $rootScope.LocationInfo.company.id,
                    'OutOfStocks': listofids,
                    'actiondate': $scope.GetBillDate()
                }
            };

            if ($scope.CurrentOutOfStock &&
                $scope.CurrentOutOfStock != null &&
                $scope.CurrentOutOfStock.id > 0) {
                request = { 'Item': JSON.parse(JSON.stringify($scope.CurrentOutOfStock)) };
                request.Item.OutOfStocks = listofids;
                request.Item.actiondate = $scope.GetBillDate();
            }

            $http({
                method: 'POST',
                url: "StoretransactionService/SaveOutOfStock",
                data: JSON.stringify(request)
            }).success(function (result) {
                $scope.RefreshOutOfStock();
                $scope.ShowOutOfStock = false;
            });
        }

        $scope.OutOfStockAll = function (item) {
            $.each(item.products, function (k, v) {
                v.outofstock = item.outofstock;
            })
            $scope.UpdateOutOfStock();
            return true;
        }

        $scope.RefreshOutOfStock = function () {
            var request = {
                "Item": {
                    'type': 'OutOfStock',
                    'locationid': $rootScope.LocationInfo.location.id,
                    'companyid': $rootScope.LocationInfo.company.id
                }
            };

            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "StoretransactionService/SearchOutOfStock",
                data: JSON.stringify(request)
            }).success(function (result) {
                $scope.CurrentOutOfStock = result;
                if ($scope.OutOfStockItems &&
                    $scope.OutOfStockItems != null &&
                    $scope.OutOfStockItems.length > 0) {
                    $.each($scope.OutOfStockItems, function (k, v) {
                        $scope.AllItems.push(v);
                        var prdgrp = $scope.TouchItems.find(function (item) {
                            return item.productgroupid == v.productgroupid;
                        });
                        if (prdgrp && prdgrp != null &&
                            prdgrp.products.indexOf(v) < 0) {
                            prdgrp.products.push(v);
                        }
                    })
                    if ($scope.TouchItems[$scope.TouchItems.length - 1].productgroupid > 0) {
                        var ofsproductgroup = { 'productgroup': 'OUT OF STOCK', 'productgroupid': -1, products: [] };
                        $scope.TouchItems.push(ofsproductgroup);
                    }
                    $scope.TouchItems[$scope.TouchItems.length - 1].products = [];
                    $scope.OutOfStockItems = $scope.TouchItems[$scope.TouchItems.length - 1].products;
                }
                else {
                    if ($scope.TouchItems[$scope.TouchItems.length - 1].productgroupid > 0) {
                        var ofsproductgroup = { 'productgroup': 'OUT OF STOCK', 'productgroupid': -1, products: [] };
                        $scope.TouchItems.push(ofsproductgroup);
                    }
                    $scope.TouchItems[$scope.TouchItems.length - 1].products = [];
                    $scope.OutOfStockItems = $scope.TouchItems[$scope.TouchItems.length - 1].products;
                }

                $.each(result.OutOfStocks, function (k, v) {
                    var product = $scope.AllItems.find(function (item) {
                        return item.productid == v;
                    });

                    if (product && product != null) {
                        product.outofstock = true;
                        $scope.OutOfStockItems.push(product);
                        var prdgrp = $scope.TouchItems.find(function (item) {
                            return item.productgroupid == product.productgroupid;
                        });
                        if (prdgrp && prdgrp != null &&
                            prdgrp.products.indexOf(product) >= 0) {
                            prdgrp.products.splice(prdgrp.products.indexOf(product), 1);
                        }
                        $scope.AllItems.splice($scope.AllItems.indexOf(product), 1);
                    }
                })

                $scope.SortProducts();

                if ($scope.ProductGroup &&
                    $scope.ProductGroup != null) {
                    $scope.Products = [];
                    $timeout(function () {
                        $scope.Products = $scope.ProductGroup.products;
                    }, 20);
                }
            });
        }

        $scope.UpdateOutOfStock = function () {
            var outstockcount = 0;
            $.each($scope.TouchItems[0].products, function (k, v) {
                if (v.outofstock)
                    outstockcount++;
            });
            $.each($scope.TouchItems[$scope.TouchItems.length - 1].products, function (k, v) {
                if (!v.outofstock)
                    outstockcount++;
            });
            $scope.OutOfStockCount = outstockcount;
        }

        $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
            typeof ($rootScope.LocationInfo.location) != 'undefined')

        $scope.GetBillDate = function () {
            if ($scope.IsDaylight == true) {
                var billdate = new Date();
                return billdate.setDate(billdate.getDate() + $scope.DaylightCount);
            }
            return new Date();
        }

        $scope.ApplicationSettings = null;
        $scope.LoadApplicationSettings = function () {
            if (!$scope.HasLocationSelected) {
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined')
                    $scope.ShowLocationInfo();
                    $scope.LoadApplicationSettings();
                });
                return;
            }

            var srequest = JSON.stringify({
                "Item": { 'locationid': $rootScope.LocationInfo.location.id, 'key': 'APPLICATIONSETTINGS' },
                "securitytoken": "String content"
            });

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SupportOutOfStock = UserDetails.Info.p_OUTOFSTOCK.view;
                if ($rootScope.LocationInfo != null &&
                    $rootScope.LocationInfo.location != null &&
                    $rootScope.LocationInfo.location.additionalinfo != null &&
                    $rootScope.LocationInfo.location.additionalinfo.autosettlement) {
                    $scope.AutoSettlement = true;
                    $rootScope.Toaster.Notice("Single window settlement enabled..");
                };
                if (result.length > 0) {
                    $scope.ApplicationSettings = result[0];
                    $scope.CurrentBilldate = $scope.GetBillDate();

                    if ($scope.ApplicationSettings.appsettings.system_datechange &&
                        $scope.ApplicationSettings.nextdaycount > 0 &&
                        $filter('date')($scope.ApplicationSettings.currentday, 'dd-MM-yyyy') == $filter('date')(new Date(), 'dd-MM-yyyy')) {
                        $scope.IsDaylight = true;
                        $scope.DaylightCount = $scope.ApplicationSettings.nextdaycount;
                        $scope.dateStyle = '#f7dcdc';
                        $scope.CurrentBilldate = $scope.GetBillDate();
                    }

                    if ($scope.ApplicationSettings.appsettings.system_saledatefreeze &&
                        $filter('date')($scope.ApplicationSettings.appsettings.system_saledatefreezeon, 'dd-MM-yyyy') != $filter('date')(new Date(), 'dd-MM-yyyy')) {
                        $scope.dateStyle = '#f7dcdc';
                        $scope.CurrentBilldate = $filter('date')($scope.ApplicationSettings.appsettings.system_saledatefreezeon, 'dd-MM-yyyy');
                    }

                    if ($scope.AutoSettlement) {
                        $scope.ValidateCounter();
                    }
                }
                else {
                    $rootScope.Toaster.Error("Please save the application setting for the current location");
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("Please save the application setting for the current location");
            });
        };
        $scope.LoadApplicationSettings();

        $scope.ValidateCounter = function () {
            if ($scope.ApplicationSettings &&
                $scope.ApplicationSettings != null &&
                $scope.ApplicationSettings.appsettings.system_cashierobcb) {
                $scope.system_cashierobcb = true;
                restService.Get('RetailReportService/Cashierstatus/' +
                    $rootScope.LocationInfo.location.id + '/' + $rootScope.LocationInfo.counter.id
                    , true).then(function (result) {

                        if (result.valid) {
                            $scope.system_cashierobcb = false;
                        }
                        else {
                            $rootScope.ShowAlert('Counter Violation', result.message);
                            $timeout(function () {
                                $(location).attr('href', 'index.html');
                            }, 1000);
                        }
                    });
            }
        }
        $scope.GetBillDate = function () {
            if ($scope.IsDaylight == true) {
                var billdate = new Date();
                return billdate.setDate(billdate.getDate() + $scope.DaylightCount);
            }
            return new Date();
        }
        $scope.AutoSettlement = false;
        $scope.OnlineCounter = false;

        $scope.AddBarcode = function (results) {
            var SalesDetail = {
                'productname': results.sellingname, "attributes": {
                    'serialno': results.barcode,
                    'mrp': results.mrp,
                    'imageid': (results.imageid && results.imageid != null && results.imageid != '') ? results.imageid : null,
                    'foodcounter': results.foodcounter
                }
            };

            SalesDetail.barcode = results.barcode;
            SalesDetail.printingname = results.sellingname;
            SalesDetail.productid = results.productid;
            SalesDetail.sellingmode = results.sellingmode;
            SalesDetail.price = results.rate;
            SalesDetail.additionaldiscountpercentage = 0;
            SalesDetail.discountpercentage = 0;
            SalesDetail.schemediscountpercentage = 0;
            SalesDetail.additionaldiscount = 0;
            SalesDetail.schemediscount = 0;
            SalesDetail.locationid = $rootScope.LocationInfo.location.id;
            SalesDetail.qty = 1;
            SalesDetail.baseprice = results.rate;
            SalesDetail.companyid = $rootScope.LocationInfo.company.id;
            SalesDetail.discount = 0;
            SalesDetail.tax = 0;
            SalesDetail.taxpercentage = 0;
            SalesDetail.taxid = results.taxid;
            SalesDetail.salerate = results.rate;

            if (results.taxid > 0) {
                SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                    return arritem.id == results.taxid;
                });
                if (typeof (SalesDetail.taxInfo) == 'undefined') {
                    SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.id == SalesDetail.taxid;
                    });
                }
                if (SalesDetail.taxInfo != null) {
                    if (SalesDetail.taxInfo.taxsplit == null) {
                        SalesDetail.taxInfo.taxsplit = { 'taxpercentage': SalesDetail.taxInfo.taxpercentage };
                    }
                    SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                }

                if (typeof (SalesDetail.taxInfo) != 'undefined' &&
                    SalesDetail.taxInfo != null) {
                    SalesDetail.taxpercentage = SalesDetail.taxInfo.taxpercentage;
                    SalesDetail.taxname = SalesDetail.taxInfo.name;
                    SalesDetail.additionaldiscount = 0;
                    SalesDetail.schemediscount = 0;

                    var baseprice = (SalesDetail.price - SalesDetail.additionaldiscount - (SalesDetail.schemediscount / SalesDetail.qty).round(2));
                    if (SalesDetail.taxInfo != null && SalesDetail.taxInfo.taxsplit != null) {
                        var taxitem = SalesDetail.taxInfo.taxsplit.findTax(baseprice);
                        SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                    }
                    SalesDetail.tax = (SalesDetail.qty * baseprice / (100 + SalesDetail.taxpercentage) * SalesDetail.taxpercentage).round(5);
                    SalesDetail.itemtax = (SalesDetail.tax / SalesDetail.qty).round(5);
                    SalesDetail.rate = (baseprice - SalesDetail.itemtax).round(5);
                    SalesDetail.piece = 1;
                    SalesDetail.saleamount = SalesDetail.qty * SalesDetail.rate;
                }
            }
            SalesDetail.receivable = results.rate * SalesDetail.qty;


            if ($scope.AllProducts && $scope.AllProducts != null) {
                var ProductDetails = $scope.AllProducts.find(function (item) {
                    return (item.id == results.productid)
                });
                if (ProductDetails && ProductDetails != null)
                    SalesDetail.sellingmode = ProductDetails.sellingmode;
            }
            return SalesDetail;
        }

        $scope.findTax = function (amount, incost) {

            if (typeof (incost) == 'undefined') incost = false;
            if (typeof (items) == 'undefined' && typeof (this.taxpercentage) != 'undefined')
                return { 'cgst': this.taxpercentage / 2, 'sgst': this.taxpercentage / 2 };

            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (incost == item.incost && item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }

        $scope.BillType = [{ id: 0, name: 'Cash' }, { id: 1, name: 'Card' }, { id: 3, name: 'Redemption' }, { id: 8, name: 'Return' }, { id: 9, name: 'Cancel' }, { id: 10, name: 'Bulk' }];

        $scope.IsPayment = false;
        $scope.showPayment = function () {
            $scope.IsPayment = !$scope.IsPayment;
        }

        $scope.IsBooked = false;
        $scope.showBooking = function () {
            $scope.IsBooked = !$scope.IsBooked;
        }

        $scope.AdditionalDetails = false;
        $scope.showAdditional = function () {
            $scope.AdditionalDetails = !$scope.AdditionalDetails;
        }

        $scope.DraftItems = [];
        $scope.Products = [];
        $scope.ProductGroup = null;
        $scope.FilterCategory = function (item, $event) {
            $('.prodcategory').removeClass('active');
            angular.element($event.currentTarget).parent().addClass('active');
            $scope.Products = item.products
            $scope.ProductGroup = item;
        }

        $scope.FilterActive = function (item, $event) {
            $('.prodcategory').removeClass('active');
            angular.element($event.currentTarget).parent().addClass('active');
        }

        $scope.deleteitem = function (item) {
            $scope.billItems.splice($scope.billItems.indexOf(item), 1);
            $scope.CalculateBillValue();
        }

        $scope.EntityBillItems = [{
            'productid': 0,
            'productname': "",
            'rate': '',
            'qty': 0,
            'discount': '',
            'tax': ''
        }];
        $scope.EntityBill = {
            'billno': '',
            'totalamount': '',
            'totaltax': ''
        };


        $scope.BuildCurrentBill = function () {
            $scope.CurrentBill = {
                "Billitems": $scope.billItems,
                'billattributes': { 'online': $scope.OnlineCounter, 'touchsale': true },
                'billtype': 0, 'isactive': true
            };


            if ($scope.ApplicationSettings.appsettings.system_datechange &&
                $scope.ApplicationSettings.nextdaycount > 0 &&
                $filter('date')($scope.ApplicationSettings.currentday, 'dd-MM-yyyy') == $filter('date')(new Date(), 'dd-MM-yyyy')) {
                $scope.IsDaylight = true;
                $scope.DaylightCount = $scope.ApplicationSettings.nextdaycount;
                $scope.dateStyle = '#f7dcdc';
                $scope.CurrentBill.billdate = $scope.GetBillDate();
            }

            if ($scope.ApplicationSettings.appsettings.system_saledatefreeze &&
                $filter('date')($scope.ApplicationSettings.appsettings.system_saledatefreezeon, 'dd-MM-yyyy') != $filter('date')(new Date(), 'dd-MM-yyyy')) {
                $scope.dateStyle = '#f7dcdc';
                $scope.CurrentBill.billdate = $filter('date')($scope.ApplicationSettings.appsettings.system_saledatefreezeon, 'dd-MM-yyyy');
            }

            $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
            $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.totalqty = 0;
            $scope.CurrentBill.totalpiece = 0;
            $scope.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
            $scope.CurrentBill.companyid = $rootScope.LocationInfo.company.id;
            $scope.CurrentBill.counterid = $rootScope.LocationInfo.counter.id;
            $scope.CurrentBill.floorid = $rootScope.LocationInfo.floorid;

            $scope.CurrentBill.countername = $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters);
            $scope.CurrentBill.floorname = $scope.SearchRef($rootScope.LocationInfo.floorid, $scope.Floors);
            $scope.CurrentBill.locationname = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
            $.each($scope.billItems, function (k, v) {
                $scope.CurrentBill.tax += v.tax;
                $scope.CurrentBill.receivable += v.receivable;
                $scope.CurrentBill.total += v.receivable;
                $scope.CurrentBill.totalqty += v.qty;
                $scope.CurrentBill.totalpiece += v.piece;
            })
        }
        $scope.Save = function (cash) {

            $scope.BuildCurrentBill();

            if (cash) {
                $scope.CurrentBill.billattributes.settlement = {
                    "isactive": true,
                    "paidamount": $scope.CurrentBill.receivable,
                    "billamount": $scope.CurrentBill.receivable,
                    "tax": $scope.CurrentBill.tax,
                    "receivable": $scope.CurrentBill.receivable,
                    "settlementon": $scope.CurrentBill.billdate,
                    "Billsettlement": [
                        {
                            "billamount": $scope.CurrentBill.receivable,
                            "receivable": $scope.CurrentBill.receivable,
                            "isactive": true,
                            "version": 1,
                            "tax": $scope.CurrentBill.tax
                        }
                    ],
                    "paymentinfo": {
                        "cash_paid": $scope.CurrentBill.receivable,
                    },
                    "floorid": $scope.CurrentBill.floorid,
                    "counterid": $scope.CurrentBill.counterid,
                    "locationid": $scope.CurrentBill.locationid
                };
                $scope.SaveBill();
            }
            else {
                if ($scope.AutoSettlement) {
                    $scope.ShowSettlement($scope.CurrentBill);
                }
                else {
                    $scope.SaveBill();
                }
            }
        }


        $scope.Addtobill = function (prodItem) {
            if ($scope.ShowOutOfStock) {
                $scope.UpdateOutOfStock();
                return;
            }

            if (prodItem.outofstock) {
                $rootScope.Toaster.Error("Out of stock");
                return;
            }

            var eitem = $scope.billItems.find(function (item) {
                return item.productid == prodItem.productid;
            });

            if (typeof (eitem) == 'undefined' || eitem == null) {
                $scope.billItems.push($scope.AddBarcode(prodItem));
            }
            else {
                eitem.qty += 1;
                eitem.piece += 1;
                eitem.tax = (eitem.qty * eitem.price / (100 + eitem.taxpercentage) * eitem.taxpercentage).round(5);
                eitem.saleamount = eitem.qty * eitem.rate;
                eitem.receivable = eitem.qty * eitem.price;
            }
            $scope.CalculateBillValue();
        }

        $scope.ChangeQty = function (eitem, value) {
            eitem.qty += value;
            eitem.piece += value;
            if ($rootScope.GetNumber(eitem.qty) <= 0) {
                $scope.deleteitem(eitem);
                return;
            }
            eitem.tax = (eitem.qty * eitem.price / (100 + eitem.taxpercentage) * eitem.taxpercentage).round(5);
            eitem.saleamount = eitem.qty * eitem.rate;
            eitem.receivable = eitem.qty * eitem.price;
            $scope.CalculateBillValue();
        };

        $scope.clearBill = function () {
            $scope.billItems = [];
            $scope.CalculateBillValue();
        }

        $scope.draftname = '';
        $scope.EditDrafrName = function (drftname) {
            $scope.draftname = drftname;
        }

        $scope.keypressOnQuery = function (command, newvalue) {
            if (command == 'backspace') {
                if ($scope.query.length > 1) {
                    $scope.query = $scope.query.substring(0, $scope.query.length - 1);
                }
                else
                    $scope.query = '';
            }
            else if (command == 'append' && newvalue == 'Space') {
                $scope.query = $scope.query + ' ';
            }
            else if (command == 'append') {
                $scope.query = $scope.query + newvalue;
            } else if (command == 'delete') {
                $scope.query = '';
            }
        }

        $scope.keypressOnPhone = function (command, newvalue) {
            if (command == 'backspace') {
                if ($scope.customerPhone.length > 1) {
                    $scope.customerPhone = $scope.customerPhone.substring(0, $scope.customerPhone.length - 1);
                }
                else
                    $scope.customerPhone = '';
            }
            else if (command == 'append') {
                $scope.customerPhone = $scope.customerPhone + newvalue;
            } else if (command == 'delete') {
                $scope.customerPhone = '';
            }
        }

        $scope.SaveBill = function () {
            var IsValid = true;
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                if ($rootScope.GetNumber(v.qty) <= 0) {
                    IsValid = false;
                }
            });

            if (!IsValid) {
                $rootScope.Toaster.Error("Please Order minimum qty, Item Qty cann't be zero.");
                return;
            }

            var request = {
                "Item": $scope.CurrentBill
            }
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "BillService/SaveTouchBill",
                data: JSON.stringify(request)
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result.id > 0) {

                    var allsaved = true;
                    $.each(result.Bills, function (k, v) {
                        if (v.id <= 0) {
                            allsaved = false;
                        } else {
                            $scope.LastBill = v;
                        }
                    });

                    if (allsaved) {
                        $scope.ErrorMessage = "Successfully saved";
                        if (print) {
                            if (result.printdocument == null || result.printdocument == '') {
                                $.each(result.Bills, function (k, v) {
                                    $scope.Print(v);
                                });
                            }
                            else {
                                $scope.Print(result);
                            }
                        }

                        $scope.ErrorMessage = "Successfully saved";
                        $scope.billItems = [];
                        $scope.CalculateBillValue();
                    }
                    else {
                        $scope.ErrorMessage = "Can't Save the bill..";
                        if (typeof (result.errormessage) != 'undefined' &&
                            result.errormessage != null && result.errormessage != '')
                            $rootScope.Toaster.Error(result.errormessage);
                    }
                }
                else {
                    $scope.ErrorMessage = result.errormessage;
                    if (typeof (result.errormessage) != 'undefined' &&
                        result.errormessage != null && result.errormessage != '')
                        $rootScope.Toaster.Error(result.errormessage);
                }
            }).error(function (result) {
                if (result && result != null &&
                    result.toString().indexOf('Out Of Stock') >= 0) {
                    $scope.RefreshOutOfStock();
                }
                $scope.SaveRequest = false;
            });
        }

        $scope.Search = function () {
            if ($scope.query &&
                $scope.query != null &&
                $scope.query.toString().startsWith('#')) {
                var request = { 'Item': { 'billno': $scope.query.toString().substring(1), 'locationid': $rootScope.LocationInfo.location.id } };
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BillService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result && result.length > 0) {
                        $scope.LastBill = result[0];
                    }
                });
            }
        }

        $scope.DirectPrint = function (item) {
            $scope.PrinterData = localStorageService.get('PrinterData');
            if ($scope.PrinterData == undefined ||
                $scope.PrinterData == null) {
                $scope.PrinterData =
                {
                    'server': null,
                    'printer': null,
                    'company': null,
                    'printerid': null,
                    'service': null,
                    'report': null,
                    'IsPDF': false,
                    'IsDirectDos': false,
                    'PaperSize': 0,
                    'IsDesign': false,
                    'isandroid': false,
                    'printertype': '',
                    'printerinfo': null
                };
            }

            var request = {
                "Item": {
                    'id': typeof (item.billid) != 'undefined' ? item.billid : item.id,
                    'enableprintdocument': true,
                    'billtype': item.billtype,
                    'isMasterBill': typeof (item.isMasterBill) == 'undefined' ? false : item.isMasterBill,
                    'isbulkreturn': item.billtype == 10 || item.billtype == 20
                },
                "SecurityToken": "String content"
            }

            request.Item.printtemplateid = $scope.PrinterData.TemplateID;
            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            request.Item.directpdf = $scope.system_saledirectpdf;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 1) {
                    $scope.Print(result[0]);
                }
            });
        };

        $scope.Print = function (item) {
            $scope.PrinterData = localStorageService.get('PrinterData');
            if ($scope.PrinterData == null) {
                $scope.PrinterData =
                {
                    'server': null,
                    'printer': null,
                    'company': null,
                    'printerid': null,
                    'service': null,
                    'report': null,
                    'IsPDF': false,
                    'IsDirectDos': false,
                    'PaperSize': 0,
                    'IsDesign': false,
                    'isandroid': false,
                    'printertype': '',
                    'printerinfo': null
                };
            }

            if (typeof (item.printdocument) != 'undefined' &&
                item.printdocument != null && item.printdocument.length > 0) {
                if (typeof (item.directpdf) != 'undefined' && item.directpdf) {
                    $rootScope.Sent2Printer(item.printdocument, $scope.PrinterData, 'PDF');
                }
                else {
                    $rootScope.Sent2Printer(item.printdocument);
                }
                return;
            }
        }


        $scope.ShowPaymentDialog = function (item) {

            if ($rootScope.UserInfo.appsettings.int_payment == undefined ||
                !$rootScope.UserInfo.appsettings.int_payment) {
                $rootScope.Toaster.Error("Payment config only supported on integrated mode only");
                return;
            }

            $scope.BuildCurrentBill();
            var dialog = ngDialog.open({
                template: 'PaymentDialog',
                data: [$scope, $scope.CurrentBill, item],
                width: '400px',
                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService', 'localStorageService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService, localStorageService) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.PaymentMode = $scope.ngDialogData[2];
                        $scope.PaymentIntegration = null;

                        var bill = $scope.ngDialogData[1];
                        if (bill == null) return;

                        $scope.PaidChanged = function (mode) {
                            $scope.Settlement.paidamount = $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) +
                                $scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) +
                                $scope.GetNumber($scope.Settlement.paymentinfo.card_paid) +
                                $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) +
                                $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid) +
                                $scope.GetNumber($scope.Settlement.paymentinfo.voucher_paid) +
                                $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid) +
                                $scope.GetNumber($scope.Settlement.paymentinfo.loan_paid);
                            $scope.Settlement.paidamount = $scope.GetNumber($scope.Settlement.paidamount);
                            $scope.Settlement.balanceamount = $scope.Settlement.receivable - $scope.Settlement.paidamount;
                            if (Math.abs($scope.Settlement.balanceamount) < 1)
                                $scope.Settlement.balanceamount = 0;
                        };

                        $scope.TemplateSettlement = {
                            "id": 0,
                            "isactive": true,
                            "createdby": 0,
                            "modifiedby": 0,
                            "version": 0,
                            "createdon": "0001-01-01T00:00:00",
                            "modifiedon": "0001-01-01T00:00:00",
                            "mode": 0,
                            "customerid": 0,
                            "paidamount": 0.0,
                            "billamount": 0.0,
                            "discount": 0.0,
                            "refundamount": 0.0,
                            "tax": 0.0,
                            "receivable": 0.0,
                            "returnbillvalue": 0.0,
                            "code": null,
                            "lastBillID": '',
                            "billmastercode": null,
                            "customername": null,
                            "customerno": null,
                            "customermobileno": null,
                            "settlementon": "0001-01-01T00:00:00",
                            "swipingmachineid": 0,
                            "Billsettlement": [],
                            "newdiscount": 0,
                            "aditionaldiscount": 0,
                            "discountapprovedby": 0,
                            "discountreason": "",
                            "paymentinfo": {
                                "cash_paid": null,
                                "has_credit": false,
                                "cash_dinamination": '',
                                "cash_return": '',
                                "cash_refund": '',
                                "card_paid": '',
                                "upi_paid": '',
                                "card_no": '',
                                "creditnote_refund": '',
                                "credit_approvedBy": 0,
                                "credit_paid": '',
                                "credit_info": '',
                                "voucher_paid": '',
                                "adjustment_enable": false,
                                "adjustment_paid": '',
                                "loyalty_paid": '',
                                "discount_enable": '',
                                "discount_approvedBy": 0,
                                "discount_paid": '',
                                "discount_info": '',
                                "return_dinamination": '',
                                "voucherinformation": ''
                            }
                        };

                        $scope.Settlement = (JSON.parse(JSON.stringify($scope.TemplateSettlement)));
                        $scope.Settlement.settlementon = bill.billdate;
                        $scope.Settlement.datedisplay = $filter('date')($scope.Settlement.settlementon, 'dd-MM-yyyy');
                        $scope.Settlement.datetimedisplay = $filter('date')($scope.Settlement.settlementon, 'hh:mm a');

                        $scope.Settlement.billmastercode = bill.billmasterid;
                        $scope.Settlement.customername = bill.customername;
                        $scope.Settlement.customermobileno = bill.phoneno;
                        $scope.Settlement.customerid = bill.customerid;

                        $scope.Settlement.tax += bill.tax;
                        $scope.Settlement.discount += bill.discount;
                        $scope.Settlement.billadditionaldiscount += bill.additionaldiscount;
                        $scope.Settlement.billamount += bill.total;
                        $scope.Settlement.receivable += bill.total;
                        $scope.Settlement.billdate = bill.billdate;
                        $scope.Settlement.balanceamount = $scope.Settlement.receivable;
                        if ($scope.Settlement.receivable < 0)
                            $scope.Settlement.returnbillvalue = Math.abs(bill.total);

                        var sbill = {};
                        sbill.companyid = bill.companyid;
                        sbill.billid = bill.id;
                        sbill.billno = bill.billno;
                        sbill.billamount = bill.receivable;
                        sbill.discount = bill.discount;
                        sbill.billadditionaldiscount = bill.additionaldiscount;
                        sbill.receivable = bill.total;
                        sbill.billdate = bill.billdate;
                        sbill.customerid = bill.customerid;
                        sbill.isactive = true;
                        sbill.iscreditbill = bill.iscreditbill || bill.billtype == 1;
                        sbill.version = 1;
                        sbill.tax = bill.tax;
                        sbill.discountreason = bill.discountrequestreason;

                        $scope.Settlement.Billsettlement.push(sbill);
                        if (typeof ($scope.Settlement.paymentinfo) == 'undefined')
                            $scope.Settlement.paymentinfo = {};

                        restService.Get('ReferencelistService/FULL-SWIPPINGMACINE', true).then(function (results) {
                            if (typeof ($rootScope.LocationInfo.location.id) != 'undefined') {
                                $scope.SwippingList = [];
                                $.each(results, function (k, v) {
                                    if (typeof (v.additionalinfo) != 'undefined' &&
                                        typeof (v.additionalinfo.locationid) != 'undefined') {
                                        if (v.additionalinfo.locationid == $rootScope.LocationInfo.location.id)
                                            $scope.SwippingList.push(v);
                                    }
                                    else {
                                        $scope.SwippingList.push(v);
                                    }
                                });
                            }
                            else {
                                $scope.SwippingList = results;
                            }
                            $timeout(function () {
                                var defaultConfig = localStorageService.get('DefaultPayment');
                                if (defaultConfig && defaultConfig != null) {
                                    $scope.PaymentIntegration = new PaymentIntegration($rootScope.UserInfo.appsettings.int_paymode,
                                        $rootScope.UserInfo.appsettings,
                                        localStorageService, $http, $timeout, $scope, $rootScope);
                                    $scope.PaymentIntegrationRunning = false;
                                    $scope.PaymentIntegration.optionid = $scope.PaymentMode.id;
                                    $scope.PaymentIntegration.CallBack = $scope.PaymentIntegrationCallBack;
                                    $scope.PaymentIntegration.EnableSingleWindowNo($rootScope.LocationInfo.location.id,
                                        $rootScope.LocationInfo.counter.id, function () {

                                            var bank = $scope.SwippingList.find(function (item) {
                                                return defaultConfig.deviceid == item.code;
                                            });
                                            if (bank == undefined || bank == null) {
                                                $rootScope.Toaster.Error("Swipping machine config missing");
                                                return;
                                            }
                                            $scope.PaymentIntegration.deviceid = bank.id;
                                            $scope.PaymentIntegration.configChanged($scope.PaymentIntegration.deviceid, $scope.SwippingList);
                                            $scope.PaymentIntegration.Integrater.Run($scope.PaymentIntegration.deviceid,
                                                $scope.PaymentIntegration.deviceconfig,
                                                $scope.PaymentIntegration.optionid, $scope.Settlement);
                                        });
                                }
                            }, 20)
                        });

                        $scope.PaymentIntegrationCallBack = function (status, response, provider) {
                            if (status == -1) {
                                $scope.PaymentIntegrationStatus = response;
                                $scope.PaymentIntegrationRunning = true;
                            }
                            if (status == 0) {
                                $scope.PaymentIntegrationStatus = response;
                                $scope.PaymentIntegrationRunning = false;
                            }
                            if (status == 1) {
                                $scope.PaymentIntegrationStatus = response;
                            }
                            if (status == 2) {
                                $scope.PaymentIntegrationStatus = response;
                                $scope.PaymentIntegrationRunning = false;
                                $rootScope.Toaster.Error(response);
                            }
                            if (status == 3) {
                                $scope.PaymentIntegrationStatus = 'Completed';
                                $scope.PaymentIntegrationRunning = false;
                                $scope.Settlement.intvendor = 2;
                                $scope.Settlement.paymentinfo.deviceresult = response;
                                $scope.Settlement.paymentinfo.paymentuniqueid = response.paymentuniqueid;
                                if (response.PaymentMode == 'CARD') {
                                    $scope.Settlement.paymentinfo.card_paid = $scope.Settlement.balanceamount;
                                    $scope.Settlement.paymentinfo.card_no = response.Card_Num;
                                }
                                else if (response.PaymentMode == 'UPI') {
                                    $scope.Settlement.paymentinfo.upi_paid = $scope.Settlement.balanceamount;
                                }
                                else if (response.PaymentMode == "EMI") {
                                    $scope.Settlement.paymentinfo.loan_paid = $scope.Settlement.balanceamount;
                                }
                                else {
                                    response.PaymentMode = "CASH";
                                    $scope.Settlement.paymentinfo.cash_paid = $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) + $scope.Settlement.balanceamount;
                                }
                                var swipmachine = $scope.SwippingList.find(function (item) {
                                    return item.id == $scope.PaymentIntegration.deviceid;
                                });
                                if (typeof (swipmachine) != 'undefined' && swipmachine != null)
                                    $scope.Settlement.paymentinfo.card_bankid = swipmachine.additionalinfo.parentid;
                                $scope.PaidChanged(response.PaymentMode);
                                $scope.Save();
                            }

                            if (status == 4) {
                                $scope.PaymentIntegrationStatus = 'Completed';
                                $scope.PaymentIntegrationRunning = false;
                                $scope.Settlement.intvendor = 1;
                                $scope.Settlement.intmode = $scope.PaymentMode.id;
                                $scope.Settlement.paymentinfo.deviceresult = response;
                                $scope.Settlement.paymentinfo.card_paid = $scope.Settlement.balanceamount;
                                $scope.Settlement.paymentinfo.card_no = result.Card_Num;
                                $scope.Settlement.paymentinfo.card_bankid = $scope.PaymentIntegration.bankid;
                                $scope.Settlement.paymentinfo.card_machineid = $scope.PaymentIntegration.deviceid;
                                $scope.PaidChanged('CARD');
                                $scope.Save();
                            }
                        }

                        $scope.Save = function () {
                            $scope.Settlement.floorid = $rootScope.LocationInfo.floorid;
                            $scope.Settlement.counterid = $rootScope.LocationInfo.counter.id;
                            $scope.Settlement.locationid = $rootScope.LocationInfo.location.id;
                            $scope.closeThisDialog($scope.Settlement);
                        }

                        $scope.Selected = function (initiate) {
                            if (initiate) {
                                $scope.PaymentIntegration.Integrater.Run($scope.PaymentIntegration.deviceid,
                                    $scope.PaymentIntegration.deviceconfig,
                                    $scope.PaymentIntegration.optionid, $scope.Settlement);
                            }
                            else {
                                $scope.PaymentIntegration.Integrater.Cancel();
                                //$scope.closeThisDialog();
                            }
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {

                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined' || $scope.CurrentBill.billattributes == null)
                            $scope.CurrentBill.billattributes = {};

                        $scope.CurrentBill.billattributes.settlement = data.value;
                        $scope.SaveBill();
                    }
                }
            });
        }

        $scope.ShowPaymentConfig = function () {

            if ($rootScope.UserInfo.appsettings.int_payment == undefined ||
                !$rootScope.UserInfo.appsettings.int_payment) {
                $rootScope.Toaster.Error("Payment config only supported on integrated mode only");
                return;
            }

            var dialog = ngDialog.open({
                template: 'PaymentConfigDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService', 'localStorageService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService, localStorageService) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.PaymentIntegration = null;
                        if ($rootScope.UserInfo.appsettings.int_payment) {
                            $scope.PaymentIntegration = new PaymentIntegration($rootScope.UserInfo.appsettings.int_paymode, $rootScope.UserInfo.appsettings,
                                localStorageService, $http, $timeout, $scope, $rootScope);
                        }


                        restService.Get('ReferencelistService/FULL-SWIPPINGMACINE', true).then(function (results) {
                            if (typeof ($rootScope.LocationInfo.location.id) != 'undefined') {
                                $scope.SwippingList = [];
                                $.each(results, function (k, v) {
                                    if (typeof (v.additionalinfo) != 'undefined' &&
                                        typeof (v.additionalinfo.locationid) != 'undefined') {
                                        if (v.additionalinfo.locationid == $rootScope.LocationInfo.location.id)
                                            $scope.SwippingList.push(v);
                                    }
                                    else {
                                        $scope.SwippingList.push(v);
                                    }
                                });
                            }
                            else {
                                $scope.SwippingList = results;
                            }
                            $scope.PaymentIntegration.Integrater.devicemode = false;
                            $scope.PaymentIntegration.Integrater.upiqrmode = false;
                            $scope.PaymentIntegration.Integrater.manualmode = false;
                            $timeout(function () {
                                var defaultConfig = localStorageService.get('DefaultPayment');
                                if (defaultConfig && defaultConfig != null) {
                                    $scope.PaymentIntegration.deviceid = defaultConfig.deviceid;
                                    $scope.PaymentIntegration.Integrater.devicemode = defaultConfig.devicemode;
                                    $scope.PaymentIntegration.Integrater.manualmode = defaultConfig.manualmode;
                                    $scope.PaymentIntegration.Integrater.upiqrmode = defaultConfig.upiqrmode;
                                    $.each($scope.PaymentIntegration.Integrater.Options, function (k, v) {
                                        var exist = defaultConfig.options.find(function (item) {
                                            return item.id == v.id;
                                        });
                                        v.selected = exist && exist != null;
                                    });
                                }
                            }, 20)
                        });
                        $scope.Selected = function () {
                            var defaultConfig = {
                                'deviceid': $scope.PaymentIntegration.deviceid,
                                'options': [],
                                'devicemode': $scope.PaymentIntegration.Integrater.devicemode,
                                'upiqrmode': $scope.PaymentIntegration.Integrater.upiqrmode,
                                'manualmode': $scope.PaymentIntegration.Integrater.manualmode
                            };
                            $.each($scope.PaymentIntegration.Integrater.Options, function (k, v) {
                                if (v.selected)
                                    defaultConfig.options.push(v);
                            });
                            localStorageService.set('DefaultPayment', defaultConfig);
                            $scope.closeThisDialog();
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {

                    }
                }
            });
        }


        $scope.UPIPaymentIntegration = null;
        $scope.UPIDialog = function () {
            if ($scope.UPIPaymentIntegration == null) {
                $scope.UPIPaymentIntegration = new PaymentIntegration($rootScope.UserInfo.appsettings.qr_paymode,
                    $rootScope.UserInfo.appsettings,
                    localStorageService, $http, $timeout, $scope, $rootScope);
                $scope.UPIPaymentIntegration.EnableSingleWindowNo($rootScope.LocationInfo.location.id,
                    $rootScope.LocationInfo.counter.id);

                $scope.TemplateSettlement = {
                    "id": 0,
                    "isactive": true,
                    "createdby": 0,
                    "modifiedby": 0,
                    "version": 0,
                    "createdon": "0001-01-01T00:00:00",
                    "modifiedon": "0001-01-01T00:00:00",
                    "mode": 0,
                    "customerid": 0,
                    "paidamount": 0.0,
                    "billamount": 0.0,
                    "discount": 0.0,
                    "refundamount": 0.0,
                    "tax": 0.0,
                    "receivable": 0.0,
                    "returnbillvalue": 0.0,
                    "code": null,
                    "lastBillID": '',
                    "billmastercode": null,
                    "customername": null,
                    "customerno": null,
                    "customermobileno": null,
                    "settlementon": "0001-01-01T00:00:00",
                    "swipingmachineid": 0,
                    "Billsettlement": [],
                    "newdiscount": 0,
                    "aditionaldiscount": 0,
                    "discountapprovedby": 0,
                    "discountreason": "",
                    "paymentinfo": {
                        "cash_paid": null,
                        "has_credit": false,
                        "cash_dinamination": '',
                        "cash_return": '',
                        "cash_refund": '',
                        "card_paid": '',
                        "upi_paid": '',
                        "card_no": '',
                        "creditnote_refund": '',
                        "credit_approvedBy": 0,
                        "credit_paid": '',
                        "credit_info": '',
                        "voucher_paid": '',
                        "adjustment_enable": false,
                        "adjustment_paid": '',
                        "loyalty_paid": '',
                        "discount_enable": '',
                        "discount_approvedBy": 0,
                        "discount_paid": '',
                        "discount_info": '',
                        "return_dinamination": '',
                        "voucherinformation": ''
                    }
                };
            }
            $scope.BuildCurrentBill();

            $scope.UPIPaymentIntegration.SetNextAutoNumber(function () {

                var dialog = ngDialog.open({
                    template: 'UPIPaymentDialog',
                    data: [$scope],
                    width: '310px',
                    controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService', 'localStorageService',
                        function ($scope, $timeout, $http, hotkeys, $rootScope, restService, localStorageService) {
                            $scope.Cancel = function () {
                                $scope.UPIPaymentIntegration.Integrater.Cancel();
                                $scope.closeThisDialog();
                            }
                            $scope.UPIPaymentIntegration = $scope.ngDialogData[0].UPIPaymentIntegration;
                            $scope.SwippingList = $scope.ngDialogData[0].SwippingList;
                            $scope.qrheight = $scope.ngDialogData[0].qrheight;
                            var bill = $scope.ngDialogData[0].CurrentBill;
                            $scope.Base64 = null;

                            $scope.Settlement = (JSON.parse(JSON.stringify($scope.ngDialogData[0].TemplateSettlement)));
                            $scope.Settlement.settlementon = bill.billdate;
                            $scope.Settlement.datedisplay = $filter('date')($scope.Settlement.settlementon, 'dd-MM-yyyy');
                            $scope.Settlement.datetimedisplay = $filter('date')($scope.Settlement.settlementon, 'hh:mm a');

                            $scope.Settlement.billmastercode = bill.billmasterid;
                            $scope.Settlement.customername = bill.customername;
                            $scope.Settlement.customermobileno = bill.phoneno;
                            $scope.Settlement.customerid = bill.customerid;

                            $scope.Settlement.tax += bill.tax;
                            $scope.Settlement.discount += bill.discount;
                            $scope.Settlement.billadditionaldiscount += bill.additionaldiscount;
                            $scope.Settlement.billamount += bill.total;
                            $scope.Settlement.receivable += bill.total;
                            $scope.Settlement.billdate = bill.billdate;
                            $scope.Settlement.balanceamount = $scope.Settlement.receivable;
                            if ($scope.Settlement.receivable < 0)
                                $scope.Settlement.returnbillvalue = Math.abs(bill.total);

                            var sbill = {};
                            sbill.companyid = bill.companyid;
                            sbill.billid = bill.id;
                            sbill.billno = bill.billno;
                            sbill.billamount = bill.receivable;
                            sbill.discount = bill.discount;
                            sbill.billadditionaldiscount = bill.additionaldiscount;
                            sbill.receivable = bill.total;
                            sbill.billdate = bill.billdate;
                            sbill.customerid = bill.customerid;
                            sbill.isactive = true;
                            sbill.iscreditbill = bill.iscreditbill || bill.billtype == 1;
                            sbill.version = 1;
                            sbill.tax = bill.tax;
                            sbill.discountreason = bill.discountrequestreason;

                            $scope.Settlement.Billsettlement.push(sbill);
                            if (typeof ($scope.Settlement.paymentinfo) == 'undefined')
                                $scope.Settlement.paymentinfo = {};

                            $scope.UPIPaymentIntegration.Integrater.Run($scope.Settlement, function (image) {
                                $scope.Base64 = image;
                            }, function (response) {

                                $scope.Settlement.intvendor = $scope.UPIPaymentIntegration.mode;
                                $scope.Settlement.paymentinfo.deviceresult = response;
                                $scope.Settlement.paymentinfo.paymentuniqueid = response.paymentuniqueid;
                                $scope.Settlement.paymentinfo.upi_paid = $scope.Settlement.balanceamount;

                                var swipmachine = $scope.SwippingList.find(function (item) {
                                    return item.code == $scope.UPIPaymentIntegration.Integrater.config.tid;
                                });
                                if (typeof (swipmachine) != 'undefined' && swipmachine != null)
                                    $scope.Settlement.paymentinfo.card_bankid = swipmachine.additionalinfo.parentid;

                                $scope.closeThisDialog($scope.Settlement);
                            });
                        }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null
                            && data.value != '$document'
                            && data.value != '$escape') {
                            $scope.CurrentBill.billattributes.settlement = data.value;
                            $scope.SaveBill();
                        }
                    }
                });

            });
        }

        $scope.ShowSettlement = function (bill) {
            var dialog = ngDialog.open({
                template: 'SettlementDialog',
                data: [$scope, bill],
                width: '650px',
                controller: 'SettlementsController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;

                    if (typeof ($scope.CurrentBill.billattributes) == 'undefined' || $scope.CurrentBill.billattributes == null)
                        $scope.CurrentBill.billattributes = {};


                    if ($scope.GetNumber(data.value.paymentinfo.card_paid) > $scope.GetNumber(data.value.receivable).round(0)
                        && $scope.GetNumber(data.value.receivable) > 0) {
                        $rootScope.Toaster.Error("Card payment can't exceed the bill value");
                        return;
                    }

                    if ($scope.GetNumber(data.value.paymentinfo.upi_paid) > $scope.GetNumber(data.value.receivable).round(0)
                        && $scope.GetNumber(data.value.receivable) > 0) {
                        $rootScope.Toaster.Error("UPI payment can't exceed the bill value");
                        return;
                    }

                    if ($scope.GetNumber(data.value.paymentinfo.upi_paid) > 0 &&
                        (data.value.paymentinfo.upiproviderid == 'undefined' ||
                            data.value.paymentinfo.upiproviderid == null ||
                            data.value.paymentinfo.upiproviderid <= 0)) {
                        if (data.value.paymentinfo.deviceresult == undefined ||
                            data.value.paymentinfo.deviceresult == null) {
                            $rootScope.Toaster.Error("Please select the UPI Provider.");
                            return;
                        }
                    }

                    $scope.CurrentBill.billattributes.settlement = data.value;
                    $scope.SaveBill();
                }
            });
        }


        $scope.qrheight = { 'height': '256px', 'width':'256px' };
        $scope.panelheight = { 'height': '400px' };
        $scope.productheight = { 'height': '400px' };
        $scope.PaymentConfig = {
            'manualmode': true, 'upiqrmode': true, 'devicemode': false, 'options': []
        };
        $timeout(function () {

            var defaultConfig = localStorageService.get('DefaultPayment');
            if (defaultConfig && defaultConfig != null) {
                $scope.PaymentConfig = defaultConfig;
            }
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 200) + 'px';
                $scope.productheight.height = '' + (available - 200) + 'px';

                //$scope.ShowVertical($rootScope.windowSize.height, $rootScope.windowSize.width);

                if ($rootScope.windowSize.height > $rootScope.windowSize.width)
                {
                    $scope.panelheight.height = '' + (available - 550) + 'px';
                    $scope.productheight.height = '' + (available - 550) + 'px';
                    $scope.qrheight = { 'height': '256px', 'width': '256px' };
                    $scope.ShowVertical($rootScope.windowSize.height, $rootScope.windowSize.width);
                }
            }
        }, 50);


        $scope.ShowAsVertical = false;
        $scope.ShowVertical = function (height, width) {
            $scope.ShowAsVertical = true;

        }

        $scope.BillValue = 0;
        $scope.CalculateBillValue = function () {
            var bv = 0;
            var bq = 0;
            $.each($scope.billItems, function (k, v) {
                bv += v.receivable;
                bq += v.piece;
            })
            $scope.BillValue = bv;
            $scope.BillQty = bq;
        }

    }]);



app.filter('filterItems', function () {
    return function (items, letter) {
        var filtered = [];
        var letterMatch = new RegExp(letter, 'i');
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            if (letterMatch.test(item.name.substring(0, 1))) {
                filtered.push(item);
            }
        }
        return filtered;
    };
});
