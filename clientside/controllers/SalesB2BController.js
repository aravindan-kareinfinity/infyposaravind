
var app = angular.module('InfinityLab');

app.controller('SalesB2BController', ['UserDetails', '$scope', '$http', '$location', '$rootScope',
    '$window', 'restService', '$filter', '$timeout', 'localStorageService', '$interval', 'hotkeys', '$stateParams', 'ngDialog', '$sce',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout, localStorageService,
        $interval, hotkeys, $stateParams, ngDialog, $sce) {
        $scope.ShowSearch = false;
        $scope.ShowImage = false;
        $scope.marginname = "Margin";
        $scope.currentimageid = '';
        $scope.SearchResult = [];
        $scope.lastBills = [];
        $scope.SearchCondition = {};
        $scope.igrid = null;
        $scope.IsDiscount = false;
        $scope.IsDaylight = false;
        $scope.dateStyle = '';
        $scope.isoffline = false;
        $scope.AllowSMChange = false;
        $scope.system_hidecost = true;
        $scope.system_b2bsalesbyproduct = false;
        $scope.system_supportbilledit = false;
        $scope.system_supportpriceedit = false;
        $scope.system_b2bexclusivetax = false;
        $scope.system_maxbarcodelength = 0;
        $scope.system_salesrounding = 0;
        $scope.rounding_rate = 2;
        $scope.rounding_tax = 4;
        $scope.rounding_discount = 2;
        $scope.rounding_amount = 2;
        $scope.sale_negativestock = false;
        $scope.system_salesroundingOnBarcode = false;
        $scope.sale_requestpasscodetosms = false;
        $scope.system_salecustomermandatory = false;
        $scope.system_salesmanmandatory = false;
        $scope.system_crsettlementonbill = false;
        $scope.system_multiplesaleqtypopup = false;
        $scope.BillNoConfig = {
            'multiple': false, 'prefix': '', 'bulkbillprefix': '', 'bulkbillprefixlist': [], 'prefixlist': [],
            'isbillno': function (key) {
                if (key.startsWith('#'))
                    return true;

                if (!this.multiple)
                    return (this.prefix != null && this.prefix != '' && key.startsWith(this.offlineprefix + this.prefix));

                for (var i = 0; i < this.prefixlist.length; i++) {
                    if (this.prefixlist[i] != null && this.prefixlist[i] != '' && key.startsWith(this.offlineprefix + this.prefixlist[i]))
                        return true;
                }

                if (!this.multiple)
                    return (this.returnprefix != null && this.returnprefix != '' && key.startsWith(this.offlineprefix + this.returnprefix));

                for (var i = 0; i < this.returnprefixlist.length; i++) {
                    if (this.returnprefixlist[i] != null && this.returnprefixlist[i] != '' && key.startsWith(this.offlineprefix + this.returnprefixlist[i]))
                        return true;
                }

                return false;
            },
            'isbulkbillno': function (key) {
                if (key.startsWith('#'))
                    return true;

                if (!this.multiple)
                    return (this.bulkbillprefix != null && this.bulkbillprefix != '' && key.startsWith(this.offlineprefix + this.bulkbillprefix));

                if (this.bulkbillprefixlist && this.bulkbillprefixlist != null) {
                    for (var i = 0; i < this.bulkbillprefixlist.length; i++) {
                        if (this.bulkbillprefixlist[i] != null && this.bulkbillprefixlist[i] != '' && key.startsWith(this.offlineprefix + this.bulkbillprefixlist[i]))
                            return true;
                    }
                }
                return false;
            },
            'init': function (ismultiple) {

                this.multiple = typeof (ismultiple) != 'undefined' || ismultiple;
                this.offlineprefix = typeof ($scope.ApplicationSettings.appsettings.system_offlineprefix) == 'undefined' || $scope.ApplicationSettings.appsettings.system_offlineprefix == null ? '' :
                    $scope.ApplicationSettings.appsettings.system_offlineprefix;

                if (this.offlineprefix == '')
                    this.offlineprefix = typeof ($scope.ApplicationSettings.appsettings.system_onlineprefix) == 'undefined' || $scope.ApplicationSettings.appsettings.system_onlineprefix == null ? '' :
                        $scope.ApplicationSettings.appsettings.system_onlineprefix;

                $scope.system_salecustomermandatory = false || $scope.ApplicationSettings.appsettings.system_salecustomermandatory;
                $scope.system_salesmanmandatory = false || $scope.ApplicationSettings.appsettings.system_salesmanmandatory;
                $scope.system_crsettlementonbill = false || $scope.ApplicationSettings.appsettings.system_crsettlementonbill;
                $scope.system_multiplesaleqtypopup = false || $scope.ApplicationSettings.appsettings.system_multiplesaleqtypopup;
                $scope.sale_negativestock = false || $scope.ApplicationSettings.appsettings.sale_negativestock;
                $scope.SOManager.Initialize($scope);
                if (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.billconfig) != 'undefined' &&
                    $rootScope.LocationInfo.billconfig != null) {

                    this.prefix = $rootScope.LocationInfo.billconfig.prefix;
                    this.prefixlist = $rootScope.LocationInfo.billconfig.prefixlist;

                    this.returnprefix = $rootScope.LocationInfo.billreturnconfig.prefix;
                    this.returnprefixlist = $rootScope.LocationInfo.billreturnconfig.prefixlist;

                    if (typeof ($rootScope.LocationInfo.bulkbillconfig) != 'undefined' && $rootScope.LocationInfo.bulkbillconfig != null) {
                        this.bulkbillprefix = $rootScope.LocationInfo.bulkbillconfig.prefix;
                        this.bulkbillprefixlist = $rootScope.LocationInfo.bulkbillconfig.prefixlist;
                    }

                    if (typeof (this.prefix) == 'undefined' || this.prefix == '')
                        this.prefix = null;

                    if (typeof (this.bulkbillprefix) == 'undefined' || this.bulkbillprefix == '')
                        this.bulkbillprefix = null;

                    if (typeof (this.bulkbillprefixlist) == 'undefined' || this.bulkbillprefixlist == '')
                        this.bulkbillprefixlist = null;
                }
            }
        };
        $scope.PromotionEnabled = false;
        $scope.AdditionalDiscountEnabled = false;
        $scope.EMIEnabled = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.DiscountQueue = false;
        $scope.IsSaleReturn = $stateParams.type == 'SALERETURN';
        $scope.IsBulkSale = $stateParams.type == 'BULKSALE';
        $scope.Title = $scope.IsSaleReturn ? 'Sales Return' : ($scope.IsBulkSale ? 'Sales On Approval' : 'Sales');
        $scope.leftPanStyle = $scope.IsSaleReturn ? '#f7dcdc' : ($scope.IsBulkSale ? '#cff7db' : 'white');
        $scope.IsWarehouse = false;
        $scope.CANAPPROVEDISCOUNT = false;
        $scope.SearchAPI = $scope.IsSaleReturn ? 'SalablegoodsService/ReturnBarcode/' : 'SalablegoodsService/Barcode/';
        $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
            typeof ($rootScope.LocationInfo.location) != 'undefined')

        $scope.BillKeys = restService.Get('ReferencelistService/BILLKEY', true).then(function (results) {
            $scope.BillKeys = results;
        });

        $scope.Promotions = restService.Get('ReferencelistService/PROMOTIONS', true).then(function (results) {
            $scope.Promotions = results;
        });

        $scope.ChangeTitle = function () {
            $scope.Title = $scope.IsSaleReturn ? 'Sales Return' : ($scope.IsBulkSale ? ($rootScope.ShowDeliveryChallan ? 'Delivery Challan' : 'Bulk Sales') : 'B2B Sales');
            if ($scope.IsDiscount && !$scope.IsBulkSale)
                $scope.Title += ' (Discount)';
        }
        $scope.ChangeTitle();

        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            $scope.CounterName = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.counter.id, results) : 'Select Counter';
        });

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

        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
            $timeout(function () {
                $scope.setFocuscustomername();
                $scope.$digest();
            }, 50);
        });

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
        }

        //$scope.SupplierProducts = null;
        //$scope.SupplierProducts = restService.Get('ReferencelistService/SUPPLIERPRODUCT', true).then(function (results) {
        //    $scope.SupplierProducts = results;
        //});
        //$scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
        //    $scope.Supplier = results;
        //});

        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.AllBrands = results;
            $scope.Brands = [];
            $.each(results, function (k, v) {
                $scope.Brands.push(v);
            });
        });

        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });
        $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $scope.AdditionalChargeList = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.FilteredProducts = [];
        $scope.Products = restService.Get('ReferencelistService/ALLPRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.SupplierChanged = function () {
            if ($scope.SupplierProducts == null || typeof ($scope.SearchCondition.SupplierID) == 'undefined' ||
                $scope.SearchCondition.SupplierID == null || $scope.SearchCondition.SupplierID == 0) {
                $scope.FilteredProducts.length = 0;
                $.each($scope.Products, function (k, v) {
                    $scope.FilteredProducts.push(v);
                });
            }
            else {
                var sp = $scope.SupplierProducts.find(function (item) {
                    return item.id == $scope.SearchCondition.SupplierID;
                });

                var productlist = typeof (sp) == 'undefined' ? [] : sp.child;
                $scope.FilteredProducts.length = 0;
                if (productlist == null || productlist.length == 0) {
                    $.each($scope.Products, function (k, v) {
                        $scope.FilteredProducts.push(v);
                    });
                }
                else {
                    $.each(productlist, function (k, v) {
                        var prod = $scope.Products.find(function (item) {
                            return item.id == v;
                        });
                        $scope.FilteredProducts.push(prod);
                    });
                }
            }
        }
        $scope.ReturnReason = restService.Get('ReferencelistService/RETURNREASON', true).then(function (results) {
            $scope.ReturnReason = results;
        });
        $scope.BulkSaleReason = restService.Get('ReferencelistService/BULKSALEREASON', true).then(function (results) {
            $scope.BulkSaleReason = results;
        });
        $scope.lastBills = restService.Get('BillmasterService/LastBills', true).then(function (results) {
            $scope.lastBills = results;
        });

        $scope.BillType = [{ id: 1, name: 'Cash' }, { id: 2, name: 'Card' }, { id: 3, name: 'Credit' }, { id: 4, name: 'Redemption' }];

        $scope.TemplateSalesDetail = {
            barcode: '',
            printingname: '',
            productid: 0,
            sellingmode: 0,
            price: '',
            invoiceitemid: '',
            id: '',
            qty: '',
            isactive: true,
            qtyeditable: false,
            priceeditable: false,
            salesmanid: '',
            salesmanname: '',
            salesmancode: '',
            version: 1
        };

        $scope.TemplateCurrentBill = {
            Billitems: [],
            additionalchargeslist: [],
            additionalcharges: 0,
            billattributes: {},
            BillTax: [],
            billdate: "",
            billtime: "",
            tax: 0.0,
            id: 0,
            isactive: true,
            iscreditbill: true,
            isbulkreturn: false,
            discount: 0.0,
            receivable: 0.0,
            additionaldiscount: 0.0,
            customerid: 0,
            phoneno: '',
            customername: 'Cash',
            billtype: 0,
            rounding: 0,
            totalqty: 0,
            totalpiece: 0,
            additionaldiscount: 0,
            total: 0,
            totaldiscount: 0,
            rounding: 0,
            schemediscount: 0,
            schemereward: 0,
            schemediscountcoupen: 0,
            HasPendingPromotion: false,
            HasPromotionApplied: false,
            HasCouponApplied: false,
            areaid: 0,
            salereasonid: 0,
            seasonid: 0,
            returnbillid: 0
        };

        $scope.ApplicationSettings = function () {

            if (!$scope.HasLocationSelected) {
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined')
                    $scope.ShowLocationInfo();
                    $scope.ApplicationSettings();
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
                if (result.length > 0) {
                    $scope.ApplicationSettings = result[0];
                    $scope.HSNLimit.init($scope.ApplicationSettings.appsettings.sale_b2bhsnlimit);
                    $scope.system_maxbarcodelength = $scope.ApplicationSettings.appsettings.system_maxbarcodelength;
                    $scope.system_b2bsalesbyproduct = $scope.ApplicationSettings.appsettings.system_b2bsalesbyproduct;
                    $scope.system_hidecost = UserDetails.Info.p_SHOWVALUE.view;
                    $scope.sale_requestpasscodetosms = $scope.ApplicationSettings.appsettings.sale_requestpasscodetosms;
                    $scope.system_salesrounding = $scope.ApplicationSettings.appsettings.system_salesrounding;
                    $scope.system_supportpriceedit = $scope.ApplicationSettings.appsettings.system_supportpriceedit;
                    $scope.system_b2bexclusivetax = $scope.ApplicationSettings.appsettings.system_b2bexclusivetax;
                    $scope.system_supportbilledit = $scope.ApplicationSettings.appsettings.system_supportbilledit;
                    $scope.system_supportshipto = false || $scope.ApplicationSettings.appsettings.system_salecustomermandatory;

                    if ($scope.system_supportpriceedit) {
                        $scope.marginname = "Price";
                    }

                    $scope.system_salesroundingOnBarcode = typeof ($scope.ApplicationSettings.appsettings.system_salesroundingOnBarcode) != 'undefined' &&
                        $scope.ApplicationSettings.appsettings.system_salesroundingOnBarcode;

                    if ($scope.ApplicationSettings.appsettings.system_datechange &&
                        $scope.ApplicationSettings.nextdaycount > 0 &&
                        $filter('date')($scope.ApplicationSettings.currentday, 'dd-MM-yyyy') == $filter('date')(new Date(), 'dd-MM-yyyy')) {
                        $scope.IsDaylight = true;
                        $scope.DaylightCount = $scope.ApplicationSettings.nextdaycount;
                        $scope.dateStyle = '#f7dcdc';
                        $scope.CurrentBill.billdate = $scope.GetBillDate();
                    }

                    $scope.BillNoConfig.init($scope.ApplicationSettings.appsettings.system_multicompany);
                    $scope.PromotionEnabled = $rootScope.License.promotions && $scope.ApplicationSettings.appsettings.sale_promotion;
                    $scope.AdditionalDiscountEnabled = $scope.ApplicationSettings.appsettings.sale_additionaldiscount;
                    $scope.EMIEnabled = $scope.ApplicationSettings.appsettings.sale_emi;
                    $scope.ExchangeEnabled = $scope.ApplicationSettings.appsettings.sale_exchange;
                    $scope.DiscountQueue = $scope.ApplicationSettings.appsettings.intray_discount;
                    $scope.IsDiscount = $scope.PromotionEnabled;
                    $scope.ManualRewards = $scope.ApplicationSettings.appsettings.sale_rewardmode == 1;
                    $scope.TemplateCurrentBill.areaid = $scope.ApplicationSettings.appsettings.sale_area;
                    $scope.TemplateCurrentBill.salereasonid = $scope.ApplicationSettings.appsettings.sale_salereason;
                    $scope.TemplateCurrentBill.seasonid = $scope.ApplicationSettings.appsettings.sale_season;
                    if ($scope.ApplicationSettings.appsettings.system_alwayscaps)
                        $scope.OnKeyPress = function (value) { return value.toUpperCase(); };
                    $scope.GridConfig();

                    if ($scope.ApplicationSettings.appsettings.system_systemrole) {
                        var authData = localStorageService.get('authorizationData');
                        if (authData.systemrole != 'ALL' &&
                            authData.systemrole != 'BILLING' &&
                            authData.systemrole != 'BILLSETTLE') {
                            $rootScope.ShowAlert('System Violation', 'This system not alloted for billing');
                            $timeout(function () {
                                $(location).attr('href', 'index.html');
                            }, 1000);
                            return;
                        }
                    }

                    if ($scope.system_b2bsalesbyproduct) {
                        restService.Get('ReferencelistService/PRODUCTBRANDS', true).then(function (results) {
                            $scope.AllBrands = results;
                            var Brands = [];
                            $.each(results, function (k, v) {
                                Brands.push(v);
                            });
                            $scope.Brands = Brands;
                        });
                    }
                }
                else {
                    $rootScope.Toaster.Error("Please save the application setting for the current location");
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("Please save the application setting for the current location");
            });
        }();

        $scope.GetBillDate = function () {
            if ($scope.IsDaylight == true) {
                var billdate = new Date();
                return billdate.setDate(billdate.getDate() + $scope.DaylightCount);
            }
            return new Date();
        }

        $scope.OnKeyPress = function (value) { return value; };
        $scope.AutoSettlement = false;

        if (!$scope.IsSaleReturn &&
            !$scope.IsBulkSale &&
            $rootScope.LocationInfo != null &&
            $rootScope.LocationInfo.location != null &&
            $rootScope.LocationInfo.location.additionalinfo != null &&
            $rootScope.LocationInfo.location.additionalinfo.autosettlement) {
            $scope.AutoSettlement = true;
            $rootScope.Toaster.Notice("Single window settlement enabled..");
        };

        $scope.CreateNew = function () {
            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
            $scope.CurrentBill = (JSON.parse(JSON.stringify($scope.TemplateCurrentBill)));
            $scope.CurrentBill.billdate = $scope.GetBillDate();
            $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
            $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
            $scope.additionaldiscountamountrequested = 0;
            $scope.additionaldiscountpercentrequested = 0;
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.lastbit = "";
            $scope.AllowAdditionalDiscount = false;
            $scope.SearchCondition.isbulkreturn = false;
            $scope.SchemeReturnBillItems = [];
            $scope.ShowImage = false;
            if (typeof ($scope.setFocuscustomername) != 'undefined')
                $scope.setFocuscustomername();
            if ($scope.igrid != null)
                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
        };

        $scope.ManageBillItem = function (action, item) {

            if (action == "Delete") {

                if (item.schemeid > 0 && $scope.IsSaleReturn) {
                    var totaldiscount = 0;
                    var totalreceivable = 0;
                    var schemeitems = [];
                    $.each($scope.SchemeReturnBillItems, function (k, v) {
                        if (v.schemeid == item.schemeid) {
                            totaldiscount += v.discount;
                            totalreceivable += (v.receivable + v.discount);
                            schemeitems.push(v);
                        }
                    });

                    var percentage = (totaldiscount / totalreceivable * 100);
                    if (schemeitems.length > 1) {
                        var index = 0;
                        $.each(schemeitems, function (k, v) {
                            index++;
                            v.schemeid = 0;
                            if (schemeitems.length == index) {
                                v.receivable = (v.receivable + v.discount) - totaldiscount;
                                v.discount = totaldiscount;
                            }
                            else {
                                var ndiscount = ((v.receivable + v.discount) * percentage / 100).round(0);
                                v.receivable = (v.receivable + v.discount) - ndiscount;
                                v.discount = ndiscount;
                                totaldiscount -= v.discount;
                            }
                        });
                    }
                }

                if (!$scope.IsSaleReturn && typeof ($scope.CurrentBill.returnbillid) != 'undefined' && item.receivable < 0) {
                    $rootScope.Toaster.Notice("Can't delete return bill items, use * to clear!.");
                    return false;
                }

                $scope.CurrentBill.Billitems.splice($scope.CurrentBill.Billitems.indexOf(item), 1);

                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    v.sno = $scope.CurrentBill.Billitems.length - k;
                });

                $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                    $scope.CurrentBill.rounding = $rootScope.RoundRupee($scope.CurrentBill.receivable.round(0), $scope.system_salesrounding) - $scope.CurrentBill.receivable.round(0);
                }
                else {
                    $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
                }
                $scope.CurrentBill.totalqty -= (item.qty).round(2);
                $scope.CurrentBill.totalpiece -= (item.piece).round(2);
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                $scope.CurrentBill.schemediscount = $scope.CurrentBill.schemediscount - item.schemediscount;
                $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty);

                var existingValue = null;
                angular.forEach($scope.CurrentBill.BillTax, function (value, key) {
                    if (value.taxid == item.taxid && value.taxpercentage == item.taxpercentage) {
                        existingValue = value;
                    }
                });

                if (existingValue != null) {
                    existingValue.amount -= ((item.salerate * item.qty));
                    existingValue.tax -= item.tax;
                    existingValue.cgst -= item.cgst;
                    existingValue.sgst -= item.sgst;
                }
                if ($scope.CurrentBill.Billitems.length == 0)
                    $scope.CurrentBill.isbulkreturn = false;
                else
                    $scope.UpdateOffer();
                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                $scope.setFocusautocoding();
                return true;
            }


            if (action == "SMChange") {
                $rootScope.Prompt('Salesman Change', 'Code', function (code) {
                    code = code.toString();
                    if (typeof (code) != 'undefined' && code.length > 0) {
                        restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + code.replace('.', ''), true).then(function (results) {
                            item.salesmancode = results.employeecode;
                            item.salesmanname = results.name;
                            item.salesmannameLast = results.name;
                            item.salesmanid = results.id;
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                        });
                    }
                })
            }

            if (action == "Edit") {

                if ($scope.IsSaleReturn) {
                    $rootScope.Prompt('Edit', 'Qty', function (count) {
                        var totalcount = $rootScope.GetNumber(count);

                        if (item.schemeid > 0 && $scope.IsSaleReturn) {
                            var totaldiscount = 0;
                            var totalreceivable = 0;
                            var schemeitems = [];
                            $.each($scope.SchemeReturnBillItems, function (k, v) {
                                if (v.schemeid == item.schemeid) {
                                    totaldiscount += v.discount;
                                    totalreceivable += (v.receivable + v.discount);
                                    schemeitems.push(v);
                                }
                            });

                            if (schemeitems.length > 1) {
                                var percentage = (totaldiscount / totalreceivable * 100) / item.qty * totalcount;
                                var index = 0;
                                $.each(schemeitems, function (k, v) {
                                    index++;
                                    v.schemeid = 0;
                                    if (schemeitems.length == index) {
                                        v.receivable = (v.receivable + v.discount) - totaldiscount;
                                        v.discount = totaldiscount;
                                    }
                                    else {
                                        var ndiscount = ((v.receivable + v.discount) * percentage / 100).round(0);
                                        v.receivable = (v.receivable + v.discount) - ndiscount;
                                        v.discount = ndiscount;
                                        totaldiscount -= v.discount;
                                    }
                                });
                            }
                        }

                        if (totalcount > 0) {

                            $timeout(function () {

                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - item.qty).round(2);
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - item.piece).round(2);

                                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - item.tax).round(5);
                                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                                $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - item.schemediscount).round(2);
                                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                                $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty)).round(2);


                                item.schemediscount = (item.schemediscount / item.qty) * totalcount * -1;
                                item.qty = totalcount * -1;
                                item.piece = totalcount * -1;

                                $scope.CurrentBill.schemediscount += item.schemediscount;
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + item.piece).round(2);
                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + item.qty).round(2);

                                $scope.UpdateItemInfo(item, false);
                                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                $scope.$digest();
                            }, 10);

                        }

                    }, item.qty * -1);
                }
                else {

                    $rootScope.Prompt('Edit', 'Qty', function (count) {
                        var totalcount = $rootScope.GetNumber(count);

                        if (totalcount > 0) {

                            $timeout(function () {

                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - item.qty).round(2);
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - item.piece).round(2);
                                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - item.tax).round(5);
                                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                                $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - item.schemediscount).round(2);
                                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                                $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty)).round(2);

                                item.schemediscount = (item.schemediscount / item.qty) * totalcount;
                                item.qty = totalcount;
                                item.piece = totalcount;

                                $scope.CurrentBill.schemediscount += item.schemediscount;
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + item.piece).round(2);
                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + item.qty).round(2);
                                $scope.UpdateItemInfo(item, false);
                                $scope.UpdateOffer();
                                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                $scope.$digest();
                            }, 10);
                        }

                    }, item.qty);

                }

                return true;
            }
        };



        $scope.UpdateItemInfo = function (saledetail, recompute) {

            if (typeof (recompute) == 'undefined')
                recompute = false;


            saledetail.qty = $scope.GetNumber(saledetail.qty);
            saledetail.piece = $scope.GetNumber(saledetail.piece);

            if (recompute) {
                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - saledetail.tax).round(5);
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - saledetail.discount).round(2);
                $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - saledetail.schemediscount).round(2);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - saledetail.receivable).round(2);
                $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (saledetail.additionaldiscount * saledetail.qty)).round(2);
            }

            if (typeof (saledetail.barcodeDetail) == 'undefined')
                saledetail.barcodeDetail = {
                    'taxid': saledetail.taxid,
                    'discount': (saledetail.discount / saledetail.qty),
                    'additionaldiscount': saledetail.additionaldiscount,
                    'stockid': saledetail.stockid,
                    'id': saledetail.salablegoodsid,
                    'companyid': saledetail.companyid
                };

            /*saledetail.discount = (saledetail.qty * (saledetail.barcodeDetail.discount + saledetail.barcodeDetail.additionaldiscount)).round(2);*/
            saledetail.discount = (saledetail.qty * (saledetail.barcodeDetail.discount)).round($scope.rounding_discount);

            if (typeof (saledetail.additionaldiscount) == 'undefined' || isNaN(saledetail.additionaldiscount))
                saledetail.additionaldiscount = 0;

            if (typeof (saledetail.schemediscount) == 'undefined' || isNaN(saledetail.schemediscount))
                saledetail.schemediscount = 0;

            saledetail.baseprice = saledetail.price - saledetail.additionaldiscount - (saledetail.schemediscount / saledetail.qty).round(5);
            if ($scope.system_b2bexclusivetax) {
                saledetail.baseprice = saledetail.etprice - $scope.GetNumber(saledetail.etdiscount);
                saledetail.itemtax = (saledetail.baseprice * saledetail.taxpercentage / 100).round(5);
            }
            else {
                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = (saledetail.baseprice / (100 + (saledetail.taxpercentage)) * 100);
                    var halfTax = (basePrice * (saledetail.taxpercentage / 2) / 100).round(5);
                    saledetail.itemtax = (halfTax * 2).round(5);
                }
                else {
                    saledetail.itemtax = (saledetail.baseprice / (100 + saledetail.taxpercentage)) * saledetail.taxpercentage;
                }
            }

            saledetail.tax = (saledetail.itemtax * saledetail.qty).round(5);
            saledetail.taxid = saledetail.barcodeDetail.taxid;

            if ($scope.system_b2bexclusivetax) {
                saledetail.rate = (saledetail.etprice - $scope.GetNumber(saledetail.etdiscount)).round(2);
                saledetail.receivable = (saledetail.rate * saledetail.qty + saledetail.tax).round(2);
            }
            else {
                saledetail.rate = (saledetail.baseprice - saledetail.itemtax).round(2);
                saledetail.receivable = (saledetail.baseprice * saledetail.qty).round(2);
            }

            saledetail.stockid = saledetail.barcodeDetail.stockid;
            saledetail.salablegoodsid = saledetail.barcodeDetail.id;
            saledetail.companyid = saledetail.barcodeDetail.companyid;

            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saledetail.tax).round(5);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saledetail.discount).round(2);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + saledetail.schemediscount).round(2);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saledetail.receivable).round(2);


            if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                $scope.CurrentBill.rounding = $rootScope.RoundRupee($scope.CurrentBill.receivable.round(0), $scope.system_salesrounding) - $scope.CurrentBill.receivable.round(0);
            }
            else {
                $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
            }

            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (saledetail.additionaldiscount * saledetail.qty)).round(2);
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

        $scope.ApplyCustomerDiscount = function () {

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid <= 0)
                return;

            if ((typeof ($scope.CurrentBill.customerdiscount) == 'undefined' || $scope.CurrentBill.customerdiscount <= 0) &&
                (typeof ($scope.CurrentBill.ccmargin) == 'undefined' || !$scope.CurrentBill.ccmarginmode))
                return;

            if ($scope.CurrentBill.discountotp && !$scope.CurrentBill.discountotpconfirmed) return;

            $.each($scope.CurrentBill.Billitems, function (k, item) {

                $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty);

                if ($scope.CurrentBill.ccmarginmode) {

                    if (item.barcodeDetail.buyingprice > 0) {
                        var newprice = item.barcodeDetail.buyingprice + (item.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100).round(0);
                        var newdiscount = (item.barcodeDetail.price - item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount) - newprice;
                        item.additionaldiscount += newdiscount;
                        item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                        item.attributes.customerdiscount = item.additionaldiscount;
                        item.baseprice -= newdiscount;
                    }
                }
                else {
                    if (typeof (item.attributes) == 'undefined' || item.attributes == null)
                        item.attributes = { 'customerdiscount': 0 };

                    var existingDiscountPercentage = (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount) / item.barcodeDetail.price * 100;

                    if ($scope.CurrentBill.billdiscountinclude) {
                        if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                            var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                            item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round(2);
                            item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                            item.attributes.customerdiscount = item.additionaldiscount;
                        }
                    }
                    else {
                        var addremaindiscount = $scope.CurrentBill.customerdiscount;
                        item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round(2);
                        item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                        item.attributes.customerdiscount = item.additionaldiscount;
                    }

                    item.discount = (item.qty * (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount)).round(2);

                    if (typeof (item.additionaldiscount) == 'undefined' || isNaN(item.additionaldiscount))
                        item.additionaldiscount = 0;

                    if (typeof (item.schemediscount) == 'undefined' || isNaN(item.schemediscount))
                        item.schemediscount = 0;

                    item.baseprice = (item.price - item.additionaldiscount - (item.schemediscount / item.qty).round(2));
                    if ($scope.system_b2bexclusivetax) {
                        item.baseprice = item.etprice - ($scope.GetNumber(item.etdiscount));
                    }
                }



                if (typeof (item.taxInfo) != 'undefined' && item.taxInfo != null && item.taxInfo.taxsplit != null) {
                    var taxitem = item.taxInfo.taxsplit.findTax(item.baseprice);
                    item.taxpercentage = taxitem.cgst + taxitem.sgst;
                }

                if ($scope.system_b2bexclusivetax) {
                    item.baseprice = item.etprice - $scope.GetNumber(item.etdiscount);
                    item.itemtax = (item.baseprice * item.taxpercentage / 100).round(5);
                }
                else {
                    if (!$scope.CurrentBill.isinterstatesale) {
                        var basePrice = (item.baseprice / (100 + (item.taxpercentage)) * 100);
                        var halfTax = (basePrice * (item.taxpercentage / 2) / 100).round(5);
                        item.itemtax = (halfTax * 2).round(5);
                    }
                    else {
                        item.itemtax = (item.baseprice / (100 + item.taxpercentage)) * item.taxpercentage;
                    }
                }

                item.tax = (item.itemtax * item.qty).round(5);
                item.taxid = item.barcodeDetail.taxid;
                item.rate = (item.baseprice - item.itemtax).round(2);
                item.receivable = (item.baseprice * item.qty).round(2);
                item.saleamount = item.qty * item.rate;

                $scope.CurrentBill.tax = $scope.CurrentBill.tax + item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount + item.discount).round(2);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + item.receivable).round(2);
                if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                    $scope.CurrentBill.rounding = $rootScope.RoundRupee($scope.CurrentBill.receivable.round(0), $scope.system_salesrounding) - $scope.CurrentBill.receivable.round(0);
                }
                else {
                    $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
                }
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount + (item.additionaldiscount * item.qty);

            });
        }

        $scope.AddBillItem = function (isb2b, pricebased) {

            if (typeof (isb2b) == 'undefined') {
                $scope.SalesDetail.additionaldiscount = 0;
                $scope.SalesDetail.additionaldiscountpercentage = 0;
            }

            if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid > 0 &&
                typeof ($scope.CurrentBill.ccmarginmode) != 'undefined' && $scope.CurrentBill.ccmarginmode &&
                (pricebased == undefined || pricebased == false)) {

                if (!$scope.CurrentBill.discountotp || $scope.CurrentBill.discountotpconfirmed) {

                    if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                        $scope.SalesDetail.attributes = { 'customerdiscount': 0 };

                    if (!$scope.system_supportpriceedit)
                        $scope.SalesDetail.price = $scope.SalesDetail.barcodeDetail.buyingprice + ($scope.SalesDetail.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100).round(0);

                    if ($scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) > 0) {
                        $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.price * $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) / 100).round(2)
                        $scope.SalesDetail.price -= $scope.SalesDetail.additionaldiscount;
                    }

                    $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.id == $scope.SalesDetail.taxid;
                    });

                    $scope.SalesDetail.price = parseFloat($scope.SalesDetail.price).round(2)
                    if (!$scope.system_supportpriceedit) {
                        if ($scope.SalesDetail.taxInfo != null) {
                            if ($scope.SalesDetail.taxInfo.taxsplit == null) {
                                $scope.SalesDetail.taxInfo.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                            }
                            $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                        }

                        if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                            var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.price);
                            $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                        }

                        if (!$scope.CurrentBill.isinterstatesale) {
                            var halfTax = ($scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 2 / 100).round(5);
                            $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                        }
                        else {
                            $scope.SalesDetail.itemtax = ($scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 100).round(5);
                        }
                        $scope.SalesDetail.rate = $scope.SalesDetail.price;
                        $scope.SalesDetail.saleamount = $scope.SalesDetail.rate * $scope.SalesDetail.qty;
                        $scope.SalesDetail.price = ($scope.SalesDetail.price + $scope.SalesDetail.itemtax).round(2);
                        $scope.SalesDetail.salerate = $scope.SalesDetail.price;
                        $scope.SalesDetail.etprice = $scope.SalesDetail.rate;
                        $scope.SalesDetail.etdiscount = 0;

                        if ($scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) > 0) {
                            $scope.SalesDetail.etdiscount = ($scope.SalesDetail.etprice * $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) / 100).round(2);
                            $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.price * $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) / 100).round(2)
                            $scope.SalesDetail.price -= $scope.SalesDetail.additionaldiscount;
                        }

                    }
                    else {
                        $scope.SalesDetail.salerate = $scope.SalesDetail.price;
                        $scope.SalesDetail.rate = $scope.SalesDetail.price - $scope.SalesDetail.itemtax;
                        $scope.SalesDetail.saleamount = $scope.SalesDetail.rate * $scope.SalesDetail.qty;
                    }
                }
            }
            else if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid > 0 &&
                typeof ($scope.CurrentBill.customerdiscount) != 'undefined' && $scope.CurrentBill.customerdiscount > 0) {

                if (!$scope.CurrentBill.discountotp || $scope.CurrentBill.discountotpconfirmed) {

                    if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                        $scope.SalesDetail.attributes = { 'customerdiscount': 0 };

                    var existingDiscountPercentage = ($scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.barcodeDetail.additionaldiscount) / $scope.SalesDetail.barcodeDetail.price * 100;

                    if ($scope.CurrentBill.billdiscountinclude) {
                        if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                            var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                            $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.barcodeDetail.price * addremaindiscount / 100).round(2);
                            $scope.SalesDetail.additionaldiscountpercentage = ($scope.SalesDetail.additionaldiscount / $scope.SalesDetail.barcodeDetail.price * 100).round(2);
                            $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                        }
                    }
                    else {
                        var addremaindiscount = $scope.CurrentBill.customerdiscount;
                        $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.barcodeDetail.price * addremaindiscount / 100).round(2);
                        $scope.SalesDetail.additionaldiscountpercentage = ($scope.SalesDetail.additionaldiscount / $scope.SalesDetail.barcodeDetail.price * 100).round(2);
                        $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                    }
                }
            }

            if ($scope.system_supportpriceedit ||
                $scope.SalesDetail.etprice != undefined)
                $scope.SalesDetail.attributes.gross = $scope.SalesDetail.etprice;
            else
                $scope.SalesDetail.attributes.gross = $scope.SalesDetail.cost + ($scope.SalesDetail.cost * $scope.GetNumber($scope.SalesDetail.margin) / 100).round(2);

            $scope.SalesDetail.discount = ($scope.SalesDetail.qty * ($scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.additionaldiscount)).round(2);

            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount)) {
                $scope.SalesDetail.additionaldiscount = 0;
                $scope.SalesDetail.additionaldiscountpercentage = 0;
            }

            if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount)) {
                $scope.SalesDetail.schemediscount = 0;
                $scope.SalesDetail.schemediscountpercentage = 0;
            }

            if ($scope.system_b2bexclusivetax) {
                $scope.SalesDetail.baseprice = $scope.SalesDetail.etprice - $scope.GetNumber($scope.SalesDetail.etdiscount);
                $scope.SalesDetail.itemtax = ($scope.SalesDetail.baseprice * $scope.SalesDetail.taxpercentage / 100).round(5);
            }
            else {
                /*$scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount - ($scope.SalesDetail.schemediscount / $scope.SalesDetail.qty).round(2));
                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = ($scope.SalesDetail.baseprice / (100 + ($scope.SalesDetail.taxpercentage)) * 100);
                    var halfTax = (basePrice * ($scope.SalesDetail.taxpercentage / 2) / 100).round(5);
                    $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                }
                else {
                    $scope.SalesDetail.itemtax = ($scope.SalesDetail.baseprice / (100 + $scope.SalesDetail.taxpercentage)) * $scope.SalesDetail.taxpercentage;
                }*/
            }

            $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(5);
            $scope.SalesDetail.taxid = $scope.SalesDetail.barcodeDetail.taxid;

            $scope.SalesDetail.rate = ($scope.SalesDetail.price - $scope.SalesDetail.itemtax).round(2);
            $scope.SalesDetail.receivable = ($scope.SalesDetail.salerate * $scope.SalesDetail.qty).round(2);
            $scope.SalesDetail.stockid = $scope.SalesDetail.barcodeDetail.stockid;
            $scope.SalesDetail.salablegoodsid = $scope.SalesDetail.barcodeDetail.id;
            $scope.SalesDetail.companyid = $scope.SalesDetail.barcodeDetail.companyid;
            $scope.SalesDetail.qty = parseFloat($scope.SalesDetail.qty);
            $scope.SalesDetail.schemediscount = 0;
            $scope.SalesDetail.schemereward = 0;
            $scope.SalesDetail.schemediscountcoupen = 0;

            if (isNaN($scope.SalesDetail.qty))
                $scope.SalesDetail.qty = 0;

            if ($scope.SalesDetail.sellingmode == 3)
                $scope.SalesDetail.piece = 1;
            else
                $scope.SalesDetail.piece = parseInt($scope.SalesDetail.qty);

            $scope.SalesDetail.saleamount = ($scope.SalesDetail.qty * $scope.SalesDetail.rate).round(2);

            if ($scope.SalesDetail.qty == 0) {
                $rootScope.Toaster.Error("Zero Qty Can't be added");
                return;
            }

            if (!$scope.HSNLimit.validate($scope.CurrentBill.Billitems, $scope.SalesDetail)) {
                $rootScope.Toaster.Error("Can't add more than " + $scope.HSNLimit.max + ' hsn codes in single bill');
                return;
            }

            $scope.CalcualteTotal($scope.SalesDetail);
            //$scope.SalesDetail.additionaldiscount = 0;
            //$scope.CurrentBill.Billitems.push($scope.SalesDetail);

            if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                $scope.SalesDetail.attributes = {};

            $scope.SalesDetail.attributes.cost = $scope.SalesDetail.cost;
            $scope.SalesDetail.attributes.margin = $scope.SalesDetail.margin;
            $scope.SalesDetail.attributes.designid = $scope.SalesDetail.designid;
            $scope.SalesDetail.attributes.brand = $scope.SalesDetail.brand;
            $scope.SalesDetail.attributes.size = $scope.SalesDetail.size;

            $scope.SalesDetail.sno = $scope.CurrentBill.Billitems.length + 1;
            $scope.CurrentBill.Billitems.splice(0, 0, $scope.SalesDetail);
            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
            $scope.UpdateOffer();
        }


        $scope.UpdateOffertimeout = null;
        $scope.UpdateOffer = function () {
            if ($scope.IsSaleReturn || !$scope.IsDiscount) return;
            if ($scope.UpdateOffertimeout != null)
                $timeout.cancel($scope.UpdateOffertimeout);
            $scope.UpdateOffertimeout = $timeout(function () {
                $scope.FindOffers();
            }, 100);
        }

        $scope.FindOffers = function (callback) {

            $scope.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
            $scope.CurrentBill.counterid = $rootScope.LocationInfo.counter.id;
            var request = {
                "Item": $scope.CurrentBill
            }
            $http({
                method: 'POST',
                url: "BillService/FindOffers",
                data: JSON.stringify(request)
            }).success(function (result) {
                if ($scope.CurrentBill.HasPendingPromotion || result.HasPendingPromotion) {
                    if ($scope.CurrentBill.Billitems.length == result.Billitems.length) {
                        $.each($scope.CurrentBill.Billitems, function (k, v) {

                            //var offerbillitem = result.Billitems.find(function(arritem) {
                            //    return arritem.barcode == v.barcode && arritem.salesmanid == v.salesmanid;
                            //});

                            offerbillitem = result.Billitems[k];
                            var lastdiscount = v.schemediscount;
                            if (typeof (offerbillitem) != 'undefined' && offerbillitem.schemediscount > 0) {
                                v.schemediscount = offerbillitem.schemediscount;
                                v.schemereward = offerbillitem.schemereward;
                                v.schemediscountcoupen = offerbillitem.schemediscountcoupen;
                                v.schemeid = offerbillitem.schemeid;
                                v.style = { "background-color": "lightgrey" };
                                v.schemediscountpercentage = ((v.schemediscount / v.qty) / (v.price + (v.discount / v.qty)) * 100).round(0);
                                if (typeof ($scope.Promotions) != 'undefined' && $scope.Promotions.length > 0) {
                                    var pinfo = $scope.Promotions.find(function (item) {
                                        return item.id == v.schemeid;
                                    });
                                    if (typeof (pinfo) != 'undefined' && pinfo != null)
                                        v.scheme = pinfo.name;
                                }
                            }
                            else {
                                v.schemediscount = 0;
                                v.schemereward = 0;
                                v.schemeid = 0;
                                v.schemediscountcoupen = 0;
                                v.scheme = '';
                                v.style = {};
                                v.schemediscountpercentage = 0;
                            }
                            if (lastdiscount != v.schemediscount) {
                                $scope.UpdateTax(v);
                                $scope.UpdateItemInfo(v, true);
                            }
                        });
                    }
                    $scope.CurrentBill.schemediscount = result.schemediscount;
                    $scope.CurrentBill.schemereward = result.schemereward;
                    $scope.CurrentBill.schemediscountcoupen = result.schemediscountcoupen;
                    $scope.CurrentBill.HasPendingPromotion = result.HasPendingPromotion;

                    if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                        $scope.CurrentBill.rounding = $rootScope.RoundRupee($scope.CurrentBill.receivable.round(0), $scope.system_salesrounding) - $scope.CurrentBill.receivable.round(0);
                    }
                    else {
                        $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
                    }
                }
                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                if (typeof (callback) != 'undefined')
                    callback();
            });
        }

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.CalcualteTotal = function (saleitem) {
            $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + saleitem.qty).round(2);
            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + saleitem.piece).round(2);
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saleitem.tax).round(5);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saleitem.discount).round(2);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + saleitem.schemediscount).round(2);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saleitem.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (saleitem.additionaldiscount * saleitem.qty)).round(2);

            if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                $scope.CurrentBill.rounding = $rootScope.RoundRupee($scope.CurrentBill.receivable.round(0), $scope.system_salesrounding) - $scope.CurrentBill.receivable.round(0);
            }
            else {
                $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
            }
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);

            var existingValue = null;

            if (typeof (saleitem.barcodeDetail) == 'undefined')
                saleitem.barcodeDetail = {
                    'taxid': saleitem.taxid,
                    'discount': (saleitem.discount / saleitem.qty),
                    'additionaldiscount': saleitem.additionaldiscount,
                    'stockid': saleitem.stockid,
                    'id': saleitem.salablegoodsid,
                    'companyid': saleitem.companyid
                };

            angular.forEach($scope.CurrentBill.BillTax, function (value, key) {
                if (existingValue == null && value.taxid == saleitem.barcodeDetail.taxid &&
                    value.taxpercentage == saleitem.taxpercentage) {
                    existingValue = value;
                }
            });

            if (existingValue == null) {
                existingValue = {
                    taxid: saleitem.barcodeDetail.taxid,
                    taxName: saleitem.taxname,
                    amount: saleitem.salerate * saleitem.qty,
                    tax: saleitem.tax,
                    taxpercentage: saleitem.taxpercentage,
                    sgst: saleitem.tax / 2,
                    cgst: saleitem.tax / 2
                }
                $scope.CurrentBill.BillTax.push(existingValue)
            }
            else {
                existingValue.amount += (saleitem.salerate * saleitem.qty);
                existingValue.tax += saleitem.tax;
                existingValue.sgst += (saleitem.tax / 2);
                existingValue.cgst += (saleitem.tax / 2);
            }

            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
        };

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            salesmanid: "",
            billmastercode: "",
            billno: null
        }

        $scope.SaveRequest = false;

        $scope.ValidateBill = function () {

            if ($scope.IsWarehouse) {
                $rootScope.Toaster.Error("Please check the sales location.");
                return false;
            }


            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return false;
            }

            if ($scope.system_salesmanmandatory) {
                var hassalesman = true;
                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    if (typeof (v.salesmanid) == 'undefined' || v.salesmanid <= 0)
                        hassalesman = false;
                });
                if (!hassalesman) {
                    $rootScope.Toaster.Notice('Please select the salesman code for all items.');
                    return false;
                }
            }


            if ($scope.IsSaleReturn) {
                $scope.CurrentBill.billtype = 8;
            }
            else if ($scope.IsBulkSale) {
                $scope.CurrentBill.billtype = 10;
                $scope.CurrentBill.isbulkreturn = true;
                if (
                    typeof ($scope.CurrentBill.billattributes) == 'undefined' ||
                    $scope.CurrentBill.billattributes == null ||
                    typeof ($scope.CurrentBill.billattributes.approvedby) == 'undefined' ||
                    $scope.CurrentBill.billattributes.approvedby == null ||
                    $scope.CurrentBill.billattributes.approvedby <= 0) {
                    $rootScope.Toaster.Notice('Please select approver');
                    $scope.setFocusbulkapprovedby();
                    return false;
                }

                if ($scope.BulkSaleReason.length > 0 &&
                    (typeof ($scope.CurrentBill.returnreasonid) == 'undefined' ||
                        $scope.CurrentBill.returnreasonid == null ||
                        $scope.CurrentBill.returnreasonid <= 0)) {
                    $rootScope.Toaster.Notice('Please select reason');
                    return false;
                }


                //if (!$rootScope.ShowDeliveryChallan) {
                if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                    $rootScope.Toaster.Notice('Please select the customer');
                    $scope.ShowCustomerDialog();
                    return false;
                }
                //}
            }
            else {
                if ($scope.CurrentBill.iscreditbill && (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0)) {
                    $rootScope.Toaster.Notice('Please select the customer');
                    $scope.ShowCustomerDialog();
                    return false;
                }
                $scope.CurrentBill.billtype = $scope.CurrentBill.iscreditbill ? 1 : 0;
            }
            $scope.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
            $scope.CurrentBill.counterid = $rootScope.LocationInfo.counter.id;
            $scope.CurrentBill.countername = $scope.CounterName;
            $scope.CurrentBill.floorname = $scope.floorname;
            $scope.CurrentBill.floorid = $rootScope.LocationInfo.floorid;
            return true;
        }
        $scope.Save = function (requestapproval, print) {

            if (!$scope.ValidateBill())
                return;

            if ($scope.CurrentBill.id > 0) {

                if ($scope.IsSaleReturn || !$scope.system_supportbilledit) {
                    $.each($scope.CurrentBill.Billitems, function (k, v) {
                        if (typeof (v.barcodeDetail) == 'undefined') {
                            v.discount = v.discount - v.schemediscount;
                            $scope.CurrentBill.discount = $scope.CurrentBill.discount - v.schemediscount;
                            v.barcodeDetail = {
                                'discount': v.discount / v.qty, 'additionaldiscount': v.additionaldiscount,
                                'taxid': v.taxid, 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid,
                                'companyid': v.companyid
                            };
                        }
                    });

                    if (!$scope.CurrentBill.HasPendingPromotion) {
                        var schemeditem = $scope.CurrentBill.Billitems.find(function (item) {
                            return item.schemediscount > 0 || item.schemeid > 0;
                        });
                        if (typeof (schemeditem) != 'undefined' && schemeditem != null) {
                            $scope.CurrentBill.HasPendingPromotion = true;
                            $rootScope.Toaster.Notice('Wait for offer updations.');
                            $scope.FindOffers(function () {
                                $rootScope.Toaster.Info('You can save now.');
                            });
                            return;
                        }
                    }


                    if ($scope.CurrentBill.billno != '') {
                        $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                        $scope.CurrentBill.returnbillid = $scope.CurrentBill.id;
                    }

                    $scope.CurrentBill.billno = '';
                    $scope.CurrentBill.id = 0;

                    if ($scope.IsBulkSale) {
                        if ($scope.CurrentBill.isbulkreturngoods) {
                            if ($scope.CurrentBill.total > 0)
                                $scope.ReverseBillValue();
                        }
                        else {
                            $scope.CurrentBill.returnbillno = '';
                        }
                    }
                }
            }


            if ($scope.AutoSettlement && (!$scope.IsSaleReturn) && (!$scope.IsBulkSale) && (!$scope.CurrentBill.iscreditbill || $scope.system_crsettlementonbill)) {
                if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                    $scope.CurrentBill.rounding = $rootScope.RoundRupee($scope.CurrentBill.receivable.round(0), $scope.system_salesrounding) - $scope.CurrentBill.receivable;
                }
                else {
                    $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
                }
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                $scope.ShowSettlement($scope.CurrentBill);
                return;
            };

            if (typeof ($scope.CurrentBill.Billitems) == 'undefined' ||
                $scope.CurrentBill.Billitems == null ||
                $scope.CurrentBill.Billitems.length == 0) {
                return;
            }

            $scope.CurrentBill.isb2b = true;

            var request = {
                "Item": $scope.CurrentBill
            }

            $scope.CurrentBill.Billitems.sort(function (x, y) {
                return x.sno < y.sno ? -1 : x.sno > y.sno ? 1 : 0;
            });

            if ($scope.SaveRequest) return;
            if (typeof ($scope.PrinterData) != 'undefined' && $scope.PrinterData != null) {
                request.Item.printtemplateid = $scope.PrinterData.TemplateID;
                request.Item.papersize = $scope.PrinterData.PaperSize;
                request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            }
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "BillService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    //if (print) {
                    //    $scope.Print(result.Bills[0]);
                    //}

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

                    if ($scope.DiscountQueue && !$scope.discounttimerenabled && $scope.CurrentBill.additionaldiscount > 0) {
                        $scope.discounttimerenabled = true;
                        $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                    }

                    $scope.AddBillHistory(result, false, true)
                    $scope.CreateNew();

                    if ($scope.DiscountQueue) {
                        $scope.AllowAdditionalDiscount = false;
                    }
                }
                else {
                    $scope.ErrorMessage = result.errormessage;
                    if (typeof (result.errormessage) != 'undefined' &&
                        result.errormessage != null && result.errormessage != '')
                        $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {
                $scope.ErrorMessage = result;
                $scope.SaveRequest = false;
            });
        }

        $scope.SearchCustomer = function () {
            if ($scope.CurrentBill.phoneno == null || $scope.CurrentBill.phoneno == "") {
                $scope.CurrentBill.customername = "Cash";
                $scope.setFocuscreditbill();
                return;
            }
            restService.Get('CustomerService/SearchByPhoneNumber?id=' + $scope.CurrentBill.phoneno + '&lid=' + $rootScope.LocationInfo.location.id, true).then(function (results) {

                if (results.id > 0) {
                    var existingCustomer = $scope.Customers.find(function (item) {
                        return item.id == results.id;
                    });
                    if (typeof (existingCustomer) == 'undefined' || existingCustomer == null)
                        $scope.Customers.push(results);
                }

                $scope.ApplyCustomerInfo(results);
            });
        }

        $scope.FilterBrands = function (history) {
            if ($scope.IsSaleReturn) {
                var availablebrands = [];
                $.each(history, function (k, v) {
                    var brand = availablebrands.find(function (item) {
                        return item.name == v.brand;
                    });
                    if (typeof (brand) == 'undefined' || brand == null) {
                        var brand = $scope.AllBrands.find(function (item) {
                            return item.name == v.brand;
                        });
                        if (typeof (brand) != 'undefined' && brand != null) {
                            availablebrands.push(brand);
                        }
                    }
                });
                if (availablebrands.length > 0) {
                    //$scope.SearchCondition.BrandID = null;
                    $scope.Brands.splice(0, $scope.Brands.length);
                    availablebrands.sort(function (first, next) {
                        return first.name == next.name;
                    });
                    $.each(availablebrands, function (k, v) {
                        $scope.Brands.push(v);
                    });
                    $scope.Brands = availablebrands;
                    $timeout(function () {
                        $scope.setFocusbrandid();
                    }, 10);
                }
                else {
                    $scope.setFocusbrandid();
                }
            }
        }


        $scope.CustomerChanged = function (nextfalse) {
            if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid != null) {
                $scope.CurrentCustomer = $scope.Customers.find(function (item) {
                    return item.id == $scope.CurrentBill.customerid;
                })
            }
            else {
                $scope.CurrentCustomer = null;
            }

            $scope.showsupplierinfo = true;
            if (typeof ($scope.CurrentCustomer) != 'undefined' && $scope.CurrentCustomer != null) {

                restService.Get('SalablegoodsService/SearchBrandPrice/' +
                    $scope.CurrentCustomer.id + '/' +
                    $rootScope.LocationInfo.location.id).then(function (results) {
                        $scope.CustomerHistory = results;
                        $scope.FilterBrands(results);
                    });

                $scope.ApplyCustomerInfo($scope.CurrentCustomer, nextfalse);
            }
            else {
                $scope.CustomerHistory = [];
                $scope.CurrentBill.customername = '';
                $scope.CurrentBill.customergst = '';
                $scope.CurrentBill.customerdiscount = 0;
                $scope.CurrentBill.ccmargin = '';
                $scope.CurrentBill.ccmarginmode = false;
                $scope.CurrentBill.phoneno = '';
            }
        }

        $scope.BrandChanged = function () {
            $scope.CurrentSelectedBrand = null;
            var brand = $scope.Brands.find(function (item) { return item.id == $scope.SearchCondition.BrandID; });
            if (brand && brand != null)
                $scope.CurrentSelectedBrand = brand.name;
        }

        $scope.ApplyCustomerInfo = function (results, nextfocus) {
            $scope.CurrentBill.phoneno = results.mobileno;
            $scope.CurrentBill.customername = results.name;
            if (results.id != $scope.CurrentBill.customerid)
                $scope.CurrentBill.customerid = results.id;
            $scope.CurrentBill.customergst = results.gstno;
            $scope.CurrentBill.customerdiscount = results.discount;

            $scope.CurrentBill.ccmargin = results.ccmargin;
            $scope.CurrentBill.ccmarginmode = results.ccmarginmode;

            $scope.CurrentBill.rewardunit = results.rewardunit;
            $scope.CurrentBill.billdiscountinclude = results.billdiscountinclude;
            $scope.CurrentBill.discountotp = results.discountotp;
            if ($scope.CurrentBill.discountotp && ($scope.CurrentBill.ccmarginmode || $scope.CurrentBill.customerdiscount > 0)) {
                $scope.CurrentBill.discountotpconfirmed = false;
                $rootScope.Toaster.Notice('Customer OTP Require for special discounts!');
                $scope.RequestCustomerOTP();
            }
            else {
                $scope.CurrentBill.discountotpconfirmed = false;
                $scope.CurrentBill.discountotp = false;
                $scope.ApplyCustomerDiscount();
            }
            if (!$scope.IsSaleReturn && (nextfocus == undefined || nextfocus == false))
                $scope.setFocusbrandid();
        }

        $scope.SearchSalesmen = function () {
            if ($scope.SalesDetail.salesmannameLast != null &&
                $scope.SalesDetail.salesmannameLast != "" &&
                $scope.SalesDetail.salesmannameLast == $scope.SalesDetail.salesmanname) {
                $scope.AddBillItem();
                $scope.setFocusBarcode();
                return;
            }

            if ($scope.SalesDetail.salesmanname == null || $scope.SalesDetail.salesmanname == '') {
                $scope.AddBillItem();
                $scope.setFocusBarcode();
            }
            else {
                restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + $scope.SalesDetail.salesmanname, true).then(function (results) {
                    $scope.SalesDetail.salesmancode = results.employeecode;
                    $scope.SalesDetail.salesmanname = results.name;
                    $scope.SalesDetail.salesmannameLast = results.name;
                    $scope.SalesDetail.salesmanid = results.id;
                });
            }
        }

        $scope.CheckLocation = function () {
            return $rootScope.LocationInfo != null;
        }

        $scope.SearchBarcode = function () {

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return;
            }

            if (($scope.SalesDetail.barcode == "0")
                && $scope.CurrentBill.Billitems.length > 0) {
                $scope.setFocusSave();
                return;
            }
            if ($scope.SalesDetail.barcode == null || $scope.SalesDetail.barcode == "") return;

            if (typeof ($scope.CurrentBill.isbulkreturn) != 'undefined' &&
                $scope.CurrentBill.isbulkreturn) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }

            if (typeof ($scope.CurrentBill.isbulkreturngoods) != 'undefined' &&
                $scope.CurrentBill.isbulkreturngoods) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }
            for (var index = 0; index < $scope.CurrentBill.Billitems.length; index++) {
                if ($scope.CurrentBill.Billitems[index].barcode == $scope.SalesDetail.barcode) {

                    if (($scope.CurrentBill.Billitems[index].qty < 0 && $scope.IsSaleReturn) ||
                        ($scope.CurrentBill.Billitems[index].qty > 0 && !$scope.IsSaleReturn)) {

                        if (!$scope.CurrentBill.Billitems[index].qtyeditable) {
                            $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]);
                        }
                        $scope.SalesDetail.barcode = "";
                        return;
                    }
                }
            }

            if (!$scope.CheckLocation())
                return;

            $scope.SalesDetail.printingname = '';
            $scope.SalesDetail.productid = 0;
            $scope.SalesDetail.sellingmode = 0;
            $scope.SalesDetail.barcodeDetail = null;
            $scope.SalesDetail.qtyeditable = false;
            $scope.SalesDetail.priceeditable = false;

            if ($scope.system_maxbarcodelength > 0 && $scope.SalesDetail.barcode.length > $scope.system_maxbarcodelength) {
                var barcodeparts = $scope.SalesDetail.barcode.split(/(\d+)/);
                if (barcodeparts[0].length == 1) {
                    $scope.SalesDetail.barcode = $scope.SalesDetail.barcode.substring(0,
                        $scope.system_maxbarcodelength + barcodeparts[0].length);
                }
                else {
                    $scope.SalesDetail.barcode = $scope.SalesDetail.barcode.substring(0,
                        $scope.system_maxbarcodelength);
                }
            }

            var barcode = $scope.SalesDetail.barcode;
            if (barcode == null || barcode == '' || barcode.indexOf(' ') >= 0 ||
                barcode.indexOf('*') >= 0 || barcode.indexOf('/') >= 0 || barcode.indexOf('\\') >= 0) {

                if (!$scope.BillNoHandling())
                    $rootScope.Toaster.Error('Unsupported barcode format');
                return;
            }
            restService.Get($scope.SearchAPI + $rootScope.LocationInfo.location.id + '/' + encodeURIComponent($scope.SalesDetail.barcode)).then(function (results) {

                if (results.errormessage != null && results.errormessage != '') {
                    $rootScope.Toaster.Error(results.errormessage);
                }

                if (results.id != 0) {

                    if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                        $scope.currentimageid = results.imageid;
                        $scope.ShowImage = true;
                    }
                    else {
                        $scope.ShowImage = false;
                    }

                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = {
                        'serialno': results.serialno, 'designid': results.designid,
                        'size': results.sizename, 'brand': results.brandname, 'typename': results.typename,
                        'colourname': results.colourname,
                        'materialname': results.materialname,
                        'stylename': results.stylename,
                        'patternname': results.patternname
                    };
                    if ($scope.SalesDetail.stock <= 0) {

                        $.confirm({
                            title: 'Duplicate',
                            content: 'This item already sold, do you want to sell it again?',
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
                                        $scope.SalesDetail.stock = 1;
                                        $scope.AddBarcode(results);


                                        $timeout(function () {
                                            $scope.setFocusSalesman();
                                            $scope.$digest();
                                        }, 10);


                                        return true;
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

                    } else {

                        //search product information
                        //if product.sellingmode != piece > enable qty editable

                        if ($scope.CurrentBill.ccmarginmode) {
                            if (results.buyingprice > 0) {
                                var newprice = results.buyingprice + (results.buyingprice * $scope.CurrentBill.ccmargin / 100).round(0);
                                var newdiscount = (results.price - results.discount + results.additionaldiscount) - newprice;
                                results.additionaldiscount += newdiscount;
                                results.additionaldiscountpercentage = (results.additionaldiscount / results.price * 100).round(2);
                                results.attributes.customerdiscount = results.additionaldiscount;
                                results.baseprice -= newdiscount;
                            }
                        }

                        $scope.AddBarcode(results);
                        //results.iqty <= 1 &&
                        if (results.price != 0)
                            $scope.setFocusSalesman();
                    }
                }
            });
        }

        $scope.UpdateTax = function (SalesDetail) {
            if (typeof (SalesDetail.taxInfo) != 'undefined' &&
                SalesDetail.taxInfo != null) {
                SalesDetail.taxpercentage = SalesDetail.taxInfo.taxpercentage;
                SalesDetail.taxname = SalesDetail.taxInfo.name;
                if (typeof (SalesDetail.additionaldiscount) == 'undefined' || isNaN(SalesDetail.additionaldiscount))
                    SalesDetail.additionaldiscount = 0;

                if (typeof (SalesDetail.schemediscount) == 'undefined' || isNaN(SalesDetail.schemediscount))
                    SalesDetail.schemediscount = 0;

                var baseprice = (SalesDetail.price - SalesDetail.additionaldiscount - (SalesDetail.schemediscount / SalesDetail.qty).round(2));
                if (SalesDetail.taxInfo != null && SalesDetail.taxInfo.taxsplit != null) {
                    var taxitem = SalesDetail.taxInfo.taxsplit.findTax(baseprice);
                    SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                }

                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = (SalesDetail.qty * baseprice / (100 + (SalesDetail.taxpercentage)) * 100);
                    var halfTax = (basePrice * (SalesDetail.taxpercentage / 2) / 100).round(5);
                    $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                }
                else {
                    SalesDetail.tax = (SalesDetail.qty * baseprice / (100 + SalesDetail.taxpercentage) * SalesDetail.taxpercentage).round(5);
                }
            }
        };

        $scope.findTax = function (amount) {

            if (typeof (items) == 'undefined' && typeof (this.taxpercentage) != 'undefined')
                return { 'cgst': this.taxpercentage / 2, 'sgst': this.taxpercentage / 2 };

            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }

        $scope.AddBarcode = function (results, autofocus) {
            $scope.SalesDetail.barcode = results.barcode;
            $scope.SalesDetail.imageid = results.imageid;
            $scope.SalesDetail.printingname = results.sellingname;
            $scope.SalesDetail.productid = results.productid;
            $scope.SalesDetail.sellingmode = results.sellingmode;
            $scope.SalesDetail.price = results.price - results.discount;
            $scope.SalesDetail.discountpercentage = (results.discount / results.price * 100).round(0);
            $scope.SalesDetail.schemediscountpercentage = 0;
            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                $scope.SalesDetail.additionaldiscount = 0;

            if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount)) {
                $scope.SalesDetail.schemediscount = 0;
                $scope.SalesDetail.additionaldiscountpercentage = 0;
            }

            $scope.SalesDetail.locationid = $rootScope.LocationInfo.location.id;

            if (typeof (results.isexchange) == 'undefined' || !results.isexchange) {
                $scope.SalesDetail.qty = $scope.SalesDetail.stock > 0 || !autofocus ? 1 : 0;

                if (results.stockisbit) {
                    $scope.SalesDetail.qty = results.stockqty;
                }
            }

            if ($scope.IsSaleReturn && $scope.SalesDetail.qty > 0) {
                $scope.SalesDetail.qty = $scope.SalesDetail.qty * 1;
            }

            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount - ($scope.SalesDetail.schemediscount / $scope.SalesDetail.qty).round(2));
            if ($scope.system_b2bexclusivetax) {
                $scope.SalesDetail.baseprice = $scope.SalesDetail.etprice - $scope.GetNumber($scope.SalesDetail.etdiscount);
            }
            $scope.SalesDetail.companyid = results.companyid;
            $scope.SalesDetail.discount = results.discount;
            $scope.SalesDetail.tax = 0;
            $scope.SalesDetail.taxpercentage = 0;
            $scope.SalesDetail.taxid = results.taxid;
            $scope.SalesDetail.receivable = $scope.SalesDetail.baseprice * $scope.SalesDetail.qty;
            $scope.SalesDetail.salerate = results.price;
            if (typeof ($scope.SalesDetail.cost) == 'undefined') {
                $scope.SalesDetail.cost = results.buyingprice;
                $scope.SalesDetail.margin = (($scope.SalesDetail.baseprice / results.buyingprice * 100) - 100).round(2);
            }

            if (results.taxid > 0) {

                if (typeof (results.isexchange) != 'undefined' && results.isexchange) {
                    $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.taxpercentage == 0;
                    });
                }
                else {
                    $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.id == results.taxid;
                    });
                }

                if ($scope.SalesDetail.taxInfo != null) {
                    if ($scope.SalesDetail.taxInfo.taxsplit == null) {
                        $scope.SalesDetail.taxInfo.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                    }
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                }
                results.taxname = $scope.SalesDetail.taxname;
                $scope.UpdateTax($scope.SalesDetail);
            }
            $scope.ProductDetails = [];
            $scope.SalesDetail.rate = $scope.SalesDetail.price - $scope.SalesDetail.itemtax;
            $scope.SalesDetail.saleamount = $scope.SalesDetail.rate * $scope.SalesDetail.qty;
            $.each($scope.Products, function (k, v) {
                if (v.id == results.productid)
                    $scope.ProductDetails.push(v);
            });


            if (!results.stockisbit && $scope.ProductDetails.length > 0
                && $scope.ProductDetails[0].sellingmode > 1 && $scope.SalesDetail.stock > 0) {
                $scope.SalesDetail.qtyeditable = true;
                $scope.SalesDetail.sellingmode = $scope.ProductDetails[0].sellingmode;
                if (autofocus) $scope.setFocusQty();
            }

            if (results.price == 0) {
                $scope.SalesDetail.priceeditable = true;
                if (autofocus) $scope.setFocusPrice();
            }

            $scope.SalesDetail.barcodeDetail = results;
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.EditBill(item);
            }
        };

        $scope.RequestDiscount = function () {
            $.confirm({
                title: 'Reason',
                content: '' +
                    '<form action="" class="formName">' +
                    '<div class="form-group">' +
                    '<input type="text" placeholder="reason" class="name form-control" required />' +
                    '</div>' +
                    '</form>',
                buttons: {
                    formSubmit: {
                        text: 'OK',
                        btnClass: 'btn-blue',
                        action: function () {
                            var name = this.$content.find('.name').val();
                            if (!name) {
                                $.alert('provide a valid name');
                                return false;
                            }
                            $scope.CurrentBill.discountrequestreason = name;
                            $scope.Save(true, false);
                            return true;
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
        };

        $scope.RejectDiscount = function (item) {
            $.confirm({
                title: 'Reason for rejection',
                content: '' +
                    '<form action="" class="formName">' +
                    '<div class="form-group">' +
                    '<input type="text" placeholder="reason" class="name form-control" required />' +
                    '</div>' +
                    '</form>',
                buttons: {
                    formSubmit: {
                        text: 'Reject',
                        btnClass: 'btn-blue',
                        action: function () {
                            var name = this.$content.find('.name').val();
                            if (!name) {
                                return false;
                            }
                            $scope.ChangeApproveStatus(item, 'R', name);
                            return true;
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
        };

        $scope.Cancel = function (item) {

            if (item.settled) {
                $rootScope.Toaster.Notice("Settled bill can't cancellation, Bill No - " + item.billno);
                return;
            }
            request = {
                "item": {
                    'ids': [item.id], 'locationid': $rootScope.LocationInfo.location.id,
                    'isbulkreturn': item.isbulkreturn,
                    'isbulksale': $scope.IsBulkSale
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Cancellation",
                data: srequest
            }).success(function (result) {
                $scope.CreateNew();
                $scope.lastBills = [];
                $rootScope.Toaster.Notice('Cancellation completed, Bill No - ' + result[0].billno);
            }).error(function (result) {
            });
        };

        $scope.PrintPDF = function (item) {

            $rootScope.Download("BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id + ($scope.IsBulkSale ? "/true" : "/false"), "bill_" + item.id + ".pdf", 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill_" + item.id + ".pdf";
                link.href = "BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id + ($scope.IsBulkSale ? "/true" : "/false");
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.Print = function (item) {
            try {
                if (typeof (item.printdocument) != 'undefined' &&
                    item.printdocument != null && item.printdocument.length > 0) {
                    if (typeof (item.directpdf) != 'undefined' && item.directpdf) {
                        $scope.Sent2PrinterPDF(item.printdocument);
                    }
                    else {
                        $scope.Sent2Printer(item.printdocument);
                    }
                    return;
                }

                var request = {
                    "Item": {
                        'id': typeof (item.billid) != 'undefined' ? item.billid : item.id,
                        'enableprintdocument': true,
                        'billtype': item.billtype, 'isbulkreturn': item.billtype == 10
                    },
                    "SecurityToken": "String content"
                }

                if (typeof ($scope.PrinterData) != 'undefined' && $scope.PrinterData != null) {
                    request.Item.printtemplateid = $scope.PrinterData.TemplateID;
                    request.Item.papersize = $scope.PrinterData.PaperSize;
                    request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BillService/Search",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length == 1) {
                        if (typeof (result[0].directpdf) != 'undefined' && result[0].directpdf) {
                            $scope.Sent2PrinterPDF(result[0].printdocument);
                        }
                        else {
                            $scope.Sent2Printer(result[0].printdocument);
                        }
                    }
                });
            } catch (e) {
                e = null;
            }
        };

        $scope.Sent2PrinterPDF = function (printdocument) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            $rootScope.Sent2Printer(printdocument, printerData, 'PDF');
            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};

            //$.ajax({
            //    type: "POST",
            //    url: printerData.service + 'PDF',
            //    data: JSON.stringify(request),
            //    contentType: 'text/plain'
            //});
        }

        $scope.Sent2PrinterRAW = function (printdocument) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            $rootScope.Sent2Printer(printdocument, printerData, 'RAW');
            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};

            //$.ajax({
            //    type: "POST",
            //    url: printerData.service + 'RAW',
            //    data: JSON.stringify(request),
            //    contentType: 'text/plain'
            //});
        }


        $scope.Sent2Printer = function (printdocument) {

            $rootScope.Sent2Printer(printdocument, printerData);

            //var printerData = localStorageService.get('PrinterData');
            //if (printerData == null) {
            //    return;
            //}
            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};
            //var srequest = JSON.stringify(request);
            //$http({
            //    method: 'POST',
            //    url: printerData.service,
            //    data: srequest
            //}).success(function (result) {

            //});
        }

        $scope.EditBill = function (item) {
            $scope.SearchCondition.billno = item.billno;
            $scope.SearchCondition.billid = item.id;
            $scope.SearchBill();
        }

        $scope.SearchBill = function (callback) {
            request = {
                "Item": {
                    'billno': $scope.SearchCondition.billno,
                    'billid': $scope.SearchCondition.billid,
                    'isbulkreturn': $scope.SearchCondition.isbulkreturn,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'removecancelled': true,
                    'isb2b': true
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {

                if (result.length == 0) {
                    if (typeof (callback) != 'undefined')
                        callback(null);
                }
                else {
                    var bill = result[0];

                    if ($scope.IsSaleReturn && bill.billtype == 9)
                        return;

                    if (typeof (callback) != 'undefined')
                        callback(bill);

                    if (!bill.settled && bill.billtype == 8) {
                        var billitems = (JSON.parse(JSON.stringify(bill.Billitems)));
                        $.each(billitems, function (k, v) {
                            var existingitem = $scope.CurrentBill.Billitems.find(function (item) {
                                return typeof (item.returnbillitemid) != 'undefined' && item.returnbillitemid == v.id;
                            });
                            if (typeof (existingitem) == 'undefined' || existingitem == null) {
                                v.returnbillitemid = v.id;
                                v.id = 0;
                                $scope.CurrentBill.Billitems.push(v);
                                $scope.CalcualteTotal(v);
                            }
                        });
                        $scope.CurrentBill.returnbillid = bill.id;
                    }
                    else {
                        $scope.CurrentBill = bill;
                        $.each(bill.Billitems, function (k, v) {
                            v.sno = k + 1;
                        });
                        $scope.additionaldiscountamountrequested = bill.additionaldiscount;
                        $scope.CurrentBill.Billitems = bill.Billitems;
                        //$scope.CurrentBill.BillTax = bill.BillTax;
                        $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
                        $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
                        //if ($scope.IsSaleReturn) {
                        //    $scope.CurrentBill.schemediscount = 0;
                        //    $.each(bill.Billitems, function (k, v) {
                        //        v.discount = (v.discount - v.schemediscount).round(2);
                        //        v.receivable = (v.receivable - v.schemediscount).round(2);
                        //        $scope.CurrentBill.total -= v.schemediscount;
                        //        $scope.CurrentBill.receivable -= v.schemediscount;
                        //        $scope.CurrentBill.discount -= v.schemediscount;
                        //        $scope.CurrentBill.schemediscount += v.schemediscount;
                        //    });
                        //}
                        $.each(bill.BillTax, function (k, v) {
                            var tax = $scope.SalesTax.find(function (item) {
                                return item.id == v.taxid;
                            });
                            v.taxName = tax != null ? tax.name : '';
                        });

                        if (typeof ($scope.CurrentBill.additionalcharges) != 'undefined' && $scope.GetNumber($scope.CurrentBill.additionalcharges) > 0) {
                            $scope.CurrentBill.total -= $scope.GetNumber($scope.CurrentBill.additionalcharges);
                        }
                    }

                    if ($scope.IsSaleReturn) {
                        $scope.ReverseBillValue();
                        $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                        $scope.CurrentBill.returnbillid = $scope.CurrentBill.id;
                        $scope.CurrentBill.billno = '';
                        $scope.CurrentBill.id = 0;
                        $scope.autoCoding.code = '';
                        $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                    }
                    $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                }
            }).error(function (result) {
            });
        };

        $scope.ReverseBillValue = function () {
            $scope.CurrentBill.totalqty = 0;
            $scope.CurrentBill.totalpiece = 0;
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            if (!$scope.IsBulkSale)
                $scope.CurrentBill.billtype = 9;
            $scope.CurrentBill.BillTax = [];
            $scope.SchemeReturnBillItems = [];

            for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {

                //var addist = $scope.CurrentBill.Billitems[i].qty * ($scope.CurrentBill.Billitems[i].additionaldiscount + $scope.CurrentBill.Billitems[i].schemediscount);
                //alert(addist);

                if ($scope.CurrentBill.Billitems[i].schemediscount > 0) {
                    $scope.CurrentBill.Billitems[i].schemediscountpercentage = (($scope.CurrentBill.Billitems[i].schemediscount / $scope.CurrentBill.Billitems[i].qty) / $scope.CurrentBill.Billitems[i].price * 100).round(0);
                }

                if ($scope.CurrentBill.Billitems[i].schemeid > 0)
                    $scope.CurrentBill.Billitems[i].style = { "background-color": "lightgrey" };

                $scope.CurrentBill.Billitems[i].qty = Math.abs($scope.CurrentBill.Billitems[i].qty) * -1;
                $scope.CurrentBill.Billitems[i].piece = Math.abs($scope.CurrentBill.Billitems[i].piece) * -1;
                $scope.CurrentBill.Billitems[i].tax = Math.abs($scope.CurrentBill.Billitems[i].tax) * -1;
                $scope.CurrentBill.Billitems[i].discount = Math.abs($scope.CurrentBill.Billitems[i].discount) * -1;
                $scope.CurrentBill.Billitems[i].receivable = Math.abs($scope.CurrentBill.Billitems[i].receivable) * -1;
                $scope.CurrentBill.Billitems[i].additionaldiscount = ($scope.CurrentBill.Billitems[i].additionaldiscount);
                $scope.CurrentBill.Billitems[i].schemediscount = Math.abs($scope.CurrentBill.Billitems[i].schemediscount) * -1;
                $scope.CurrentBill.Billitems[i].saleamount = $scope.CurrentBill.Billitems[i].qty * $scope.CurrentBill.Billitems[i].salerate;

                //var addist = $scope.CurrentBill.Billitems[i].qty * ($scope.CurrentBill.Billitems[i].additionaldiscount + $scope.CurrentBill.Billitems[i].schemediscount);
                //alert(addist);
                $scope.CalcualteTotal($scope.CurrentBill.Billitems[i]);

                if ($scope.CurrentBill.Billitems[i].schemeid > 0)
                    $scope.SchemeReturnBillItems.push($scope.CurrentBill.Billitems[i]);
            }
            if ($scope.SchemeReturnBillItems.length > 0) {
                $rootScope.Toaster.Notice('Return bill contains offer items, final return value may differ');
            }
        }

        $scope.Search = function () {
            $scope.SearchCondition.removecancelled = true;
            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });
        };


        $scope.ChangeApproveStatus = function (item, status, reason) {
            var request = null;
            if (status == 'A') {
                request = {
                    "Item": { 'id': item.id, 'status': 3 },
                    "SecurityToken": "String content"
                }
            } else if (status == 'R') {
                request = {
                    "Item": { 'id': item.id, 'status': 2, 'actionreason': reason },
                    "SecurityToken": "String content"
                }
            } else if (status == 'X') {
                request = {
                    "Item": { 'id': item.id, 'hasaccepted': true },
                    "SecurityToken": "String content"
                }
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });
        }

        $scope.AddBillHistory = function (result, isdiscount, isupdate) {
            if (typeof ($scope.lastBills) == 'undefined' || $scope.lastBills == null)
                $scope.lastBills = [];
            if (isdiscount) {
                $.each(result, function (k, v) {
                    var existbill = $scope.lastBills.find(function (item) {
                        return item.type == 'B' && item.id == v.billid;
                    });
                    if (existbill != null && existbill.id > 0) {
                        existbill.discountstatus = v.discountstatus;
                    }
                    else {
                        var existbill = $scope.lastBills.find(function (item) {
                            return item.type == 'D' && item.id == v.id;
                        });
                        if (existbill != null && existbill.id > 0) {
                            existbill.discountstatus = v.status;
                        }
                        else {
                            v.type = 'D';
                            v.discountstatus = v.status;
                            $scope.lastBills.splice(0, 0, v);
                        }
                    }
                })
            }
            else {

                $scope.lastBills = [];
                var listofnormalbill = [];
                if (typeof ($scope.lastBills) == 'undefined' || $scope.lastBills == null)
                    $scope.lastBills = [];

                $.each($scope.lastBills, function (k, v) {
                    if (v.type == 'B' && v.discountstatus == 0)
                        listofnormalbill.push(v);
                    if (v.type == 'D' && isupdate && result.id == v.billid)
                        listofnormalbill.push(v);
                });


                $.each(listofnormalbill, function (k, v) {
                    $scope.lastBills.splice($scope.lastBills.indexOf(v), 1);
                });

                if (result.Bills) {
                    $.each(result.Bills, function (k, v) {
                        v.type = 'B';
                        //$scope.lastBills.push(v);
                        $scope.lastBills.splice(0, 0, v);
                    });
                }
                else {
                    $scope.lastBills.splice(0, 0, result);
                }
            }
        }

        $scope.discounttimerenabled = false;
        $scope.CheckDiscountRequest = function () {
            var request = {
                "Item": {
                    'requestedby': 1,
                    'mode': $scope.CanApprove ? 2 : 1
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    request.Item.fromid = result[result.length - 1].id;
                    $scope.AddBillHistory(result, true, false);
                    if ($scope.DiscountQueue) {
                        $scope.discounttimerenabled = true;
                        $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                    }
                }
            });
        }

        if ($scope.DiscountQueue) {
            $scope.CheckDiscountRequest();
        }

        $scope.PullDiscountRequest = function (request) {
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    request.Item.fromid = result[result.length - 1].id;
                    $scope.AddBillHistory(result, true, false);
                }
            });
        }

        $scope.CanApprove = true;
        $scope.AllowAdditionalDiscount = false;
        $scope.ShowDiscountApprove = false;
        $scope.ShowDiscountQueue = function (enable) {

        };

        //$scope.ApplyDiscount = function (ispercentage) {
        //    if (ispercentage) {
        //        var percentage = parseFloat($scope.additionaldiscountpercentrequested);
        //        if (percentage < 0 || percentage > 100)
        //            percentage = 0;
        //        discountamount = 0;
        //        var tax = 0;
        //        $.each($scope.CurrentBill.Billitems, function (k, v) {

        //            var existingTaxValue = $scope.CurrentBill.billTax.find(function (item) {
        //                return item.taxid == v.taxid;
        //            });

        //            existingTaxValue.amount -= ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax -= (v.tax);

        //            v.additionaldiscount = Math.round(v.price * percentage / 100, 0);
        //            v.additionaldiscountpercentage = percentage;
        //            discountamount += v.additionaldiscount;
        //            $scope.UpdateTax(v);
        //            tax += v.tax;

        //            existingTaxValue.amount += ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax += (v.tax);

        //        });
        //        $scope.CurrentBill.additionaldiscount = discountamount;
        //        $scope.additionaldiscountamountrequested = discountamount;
        //        $scope.CurrentBill.tax = tax;
        //    }
        //    else {
        //        var discountamount = parseFloat($scope.additionaldiscountamountrequested);
        //        if (discountamount < 0)
        //            discountamount = 0;
        //        $scope.CurrentBill.additionaldiscount = discountamount;
        //        percentage = ((discountamount / $scope.CurrentBill.receivable) * 100);
        //        var maxitem = null;
        //        var tax = 0;
        //        $.each($scope.CurrentBill.Billitems, function (k, v) {
        //            if (maxitem == null || maxitem.price < v.price)
        //                maxitem = v;

        //            var existingTaxValue = $scope.CurrentBill.billTax.find(function (item) {
        //                return item.taxid == v.taxid;
        //            });

        //            existingTaxValue.amount -= ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax -= (v.tax);

        //            v.additionaldiscount = Math.round(v.price * percentage / 100, 0);
        //            v.additionaldiscountpercentage = percentage;
        //            discountamount -= v.additionaldiscount;
        //            $scope.UpdateTax(v);

        //            existingTaxValue.amount += ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax += (v.tax);
        //            tax += v.tax;
        //        });

        //        if (discountamount < 0)
        //            maxitem.additionaldiscount -= discountamount;
        //        else
        //            maxitem.additionaldiscount += discountamount;
        //        //maxitem.receivable = (maxitem.price - maxitem.additionaldiscount) * maxitem.qty;
        //        $scope.CurrentBill.tax = tax;
        //        $scope.additionaldiscountpercentrequested = (percentage).round(2);
        //    }
        //}

        $scope.autoCoding = { 'code': '' };

        $scope.ValidateReturnAndSave = function () {
            if (($scope.CurrentBill.billtype == 8 || $scope.CurrentBill.billtype == 9) &&
                $scope.CurrentBill.receivable < 0) {
                $scope.Save(false, true);
            }
        }

        $scope.ShowCancellDialog = function (bill) {
            $rootScope.Confirm('Delete Bulk Bill', 'Do you want to cancel the bulk bill - ' + bill.billno, function () {
                bill.isbulkreturn = true;
                var request = {
                    "Item": bill
                }
                $http({
                    method: 'POST',
                    url: "BillService/Delete",
                    data: JSON.stringify(request)
                }).success(function (result) {
                    $scope.CreateNew();
                });
            });
        };


        $scope.HasSuggested = function () {
            if ($scope.suggest_selected != null && $scope.autoCoding.code == $scope.suggest_selected.name) {
                switch ($scope.suggest_selected.type) {
                    case 'AREA':
                        $scope.CurrentBill.areaid = $scope.suggest_selected.id;
                        $scope.CurrentBill.areaname = $scope.suggest_selected.name;
                        break;
                    case 'BULKSALEREASON':
                        $scope.CurrentBill.salereasonid = $scope.suggest_selected.id;
                        $scope.CurrentBill.salereason = $scope.suggest_selected.name;
                        break;
                    case 'SEASONS':
                        $scope.CurrentBill.seasonid = $scope.suggest_selected.id;
                        $scope.CurrentBill.season = $scope.suggest_selected.name;
                        break;
                }
                $scope.CurrentBill.displaykey = '';
                if (typeof ($scope.CurrentBill.areaname) != 'undefined')
                    $scope.CurrentBill.displaykey += $scope.CurrentBill.areaname;
                if (typeof ($scope.CurrentBill.salereason) != 'undefined')
                    $scope.CurrentBill.displaykey += ($scope.CurrentBill.displaykey.length > 0 ? ',' : '') + $scope.CurrentBill.salereason;
                if (typeof ($scope.CurrentBill.season) != 'undefined')
                    $scope.CurrentBill.displaykey += ($scope.CurrentBill.displaykey.length > 0 ? ',' : '') + $scope.CurrentBill.season;
                return true;
            }
            return false;
        }

        $scope.SearchAutoCoding = function () {

            if ($scope.autoCoding.code == null || $scope.autoCoding.code == "") return;

            if ($scope.HasSuggested()) {
                $scope.autoCoding.code = '';
                $scope.setFocusautocoding();
            }

            if (($scope.autoCoding.code == "0")
                && $scope.CurrentBill.Billitems.length > 0) {

                if ($scope.IsSaleReturn &&
                    (typeof ($scope.CurrentBill.returnbillno) == 'undefined' || $scope.CurrentBill.returnbillno == '')) {
                    $rootScope.Toaster.Notice('Please use sales return screen for returns.');
                    $scope.setFocusreturnbillno();
                }
                else {
                    $scope.Save(false, true);
                    $scope.autoCoding.code = '';
                }
                return;
            }

            if ($scope.autoCoding.code == "P" || $scope.autoCoding.code == "p") {
                if ($scope.CurrentBill.id > 0) {
                    $scope.Print($scope.CurrentBill);
                    $scope.autoCoding.code = '';
                }
                return;
            }

            if ($scope.autoCoding.code == "R" || $scope.autoCoding.code == "r") {
                if (!$scope.IsSaleReturn) {
                    $rootScope.Toaster.Notice('Please use sales return screen for returns.');
                    return;
                }
                $scope.Save(false, true);
                $scope.autoCoding.code = '';
                return;
            }


            if ($scope.autoCoding.code == "m" || $scope.autoCoding.code == "M") {
                $scope.autoCoding.code = '';
                $scope.CurrentBill.id = 0;
                $scope.CurrentBill.billno = '';
                return;
            }

            if ($scope.autoCoding.code == "d" || $scope.autoCoding.code == "D") {
                $scope.autoCoding.code = '';
                $scope.ShowAdditionalDiscount();
                return;
            }

            if ($scope.autoCoding.code == "-"
                && $scope.CurrentBill.Billitems.length > 0) {
                $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[0]);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code.startsWith("-") && $scope.autoCoding.code.length > 1
                && $scope.CurrentBill.Billitems.length > 0) {
                var lastdiscountisvalue = $scope.autoCoding.code.endsWith("-");
                var lastdiscount = $scope.GetNumber($scope.autoCoding.code.replace('-', ''));
                if (!$rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view) {
                    $rootScope.Toaster.Notice("You dont' have rights to give direct discount!");
                    $scope.autoCoding.code = '';
                    return;
                }

                if (lastdiscount > parseFloat($rootScope.UserInfo.AD)) {
                    $rootScope.Toaster.Notice("Your allowed discount - " + lastdiscount + '%');
                    $scope.autoCoding.code = '';
                    return;
                }

                $scope.ApplySingleRowDiscount($scope.CurrentBill.Billitems[0], lastdiscountisvalue ? '' : '%', lastdiscount);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code == "*") {
                $scope.CreateNew();
                return;
            }

            if ($scope.autoCoding.code == "N" || $scope.autoCoding.code == "n") {
                $scope.CreateNew();
                return;
            }

            if ($scope.autoCoding.code.length == 10) {
                var letters = /^[0-9]+$/;
                if ($scope.autoCoding.code.match(letters)) {
                    $scope.CurrentBill.phoneno = $scope.autoCoding.code;
                    $scope.autoCoding.code = '';
                    $scope.SearchCustomer();
                    return;
                }
            }

            if ($scope.autoCoding.code == 'C' || $scope.autoCoding.code == 'c') {
                $scope.ShowCustomerDialog();
                return;
            }

            if ($scope.autoCoding.code == 'L' || $scope.autoCoding.code == 'l') {
                if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                    $rootScope.Toaster.Notice('Please select the customer');
                    return;
                }

                if ($scope.CurrentBill.receivable == 0) {
                    $rootScope.Toaster.Notice('Please enter the barcode');
                    return;
                }

                $scope.ApplyRewardDialog();
                return;
            }



            if ($scope.autoCoding.code.startsWith('.') && $scope.autoCoding.code.length > 0) {
                restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + $scope.autoCoding.code.replace('.', ''), true).then(function (results) {
                    if (results.id == 0) return;

                    if (typeof ($scope.CurrentBill.Billitems[0].salesmanid) == 'undefined' ||
                        $scope.CurrentBill.Billitems[0].salesmanid == null) {
                        for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                            if (typeof ($scope.CurrentBill.Billitems[i].salesmanid) == 'undefined' ||
                                $scope.CurrentBill.Billitems[i].salesmanid == null) {
                                $scope.CurrentBill.Billitems[i].salesmancode = results.employeecode;
                                $scope.CurrentBill.Billitems[i].salesmanname = results.name;
                                $scope.CurrentBill.Billitems[i].salesmanid = results.id;
                                $scope.autoCoding.code = '';
                            }
                        }
                    }
                    else {
                        var replacesmid = $scope.CurrentBill.Billitems[0].salesmanid;
                        for (var i = $scope.CurrentBill.Billitems.length - 1; i >= 0; i--) {
                            if ($scope.CurrentBill.Billitems[i].salesmanid == replacesmid) {
                                $scope.CurrentBill.Billitems[i].salesmancode = results.employeecode;
                                $scope.CurrentBill.Billitems[i].salesmanname = results.name;
                                $scope.CurrentBill.Billitems[i].salesmanid = results.id;
                                $scope.autoCoding.code = '';
                            }
                        }
                    }
                });
                return;
            }

            for (var index = 0; index < $scope.CurrentBill.Billitems.length; index++) {
                if ($scope.CurrentBill.Billitems[index].barcode == $scope.autoCoding.code &&
                    (typeof ($scope.CurrentBill.Billitems[index].salesmanid) == 'undefined'
                        || $scope.CurrentBill.Billitems[index].salesmanid == 0
                        || $scope.CurrentBill.Billitems[index].sellingmode == 1)) {

                    if ($scope.CurrentBill.Billitems[index].sellingmode == 1) {
                        if (typeof ($scope.CurrentBill.Billitems[index].stock) != 'undefined' && $scope.CurrentBill.Billitems[index].stock <= 1) {
                            if ($scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]))
                                $rootScope.Toaster.Notice('Deleted');

                            $scope.autoCoding.code = "";
                            return;
                        }
                        else {

                            var newQty = $scope.IsSaleReturn ? -1 : 1;

                            $scope.CurrentBill.Billitems[index].qty += newQty;
                            $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + newQty).round(2);
                            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);
                            if ($scope.CurrentBill.Billitems[index].sellingmode == 3) {
                                $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].piece) + newQty;
                            }
                            else {
                                $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].qty);
                            }
                            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                            $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                            $scope.autoCoding.code = "";
                            $rootScope.Toaster.Notice('Qty increased to ' + $scope.CurrentBill.Billitems[index].piece);

                            if ($scope.CurrentBill.Billitems[index].piece == 10 && $scope.system_multiplesaleqtypopup) {
                                $timeout(function () {
                                    $rootScope.Prompt('Multiple', 'No Of Pieces', function (count) {
                                        var totalcount = $rootScope.GetNumber(count);
                                        if (totalcount > 0) {

                                            $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - $scope.CurrentBill.Billitems[index].qty).round(2);
                                            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                                            $scope.CurrentBill.Billitems[index].qty = totalcount;
                                            $scope.CurrentBill.Billitems[index].piece = totalcount;

                                            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                                            $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + $scope.CurrentBill.Billitems[index].qty).round(2);

                                            $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                                            $rootScope.Toaster.Notice('Qty altered to ' + $scope.CurrentBill.Billitems[index].piece);
                                            $scope.UpdateOffer();
                                        }
                                    }, $scope.CurrentBill.Billitems[index].piece);

                                }, 25);

                            }
                            else {
                                $scope.UpdateOffer();
                            }
                            return;
                        }
                    }
                    if ($scope.CurrentBill.Billitems[index].sellingmode == 2) {

                        var newQty = $scope.IsSaleReturn ? -1 : 1;

                        $scope.CurrentBill.Billitems[index].qty += newQty;
                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + newQty).round(2);
                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                        if ($scope.CurrentBill.Billitems[index].sellingmode == 3) {
                            $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].piece) + newQty;
                        }
                        else {
                            $scope.CurrentBill.Billitems[index].piece = parseInt($scope.CurrentBill.Billitems[index].qty);
                        }
                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                        $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                        $scope.autoCoding.code = "";
                        $rootScope.Toaster.Notice('Qty increased to ' + $scope.CurrentBill.Billitems[index].qty);


                        if ($scope.CurrentBill.Billitems[index].piece == 3 && $scope.system_multiplesaleqtypopup) {

                            $timeout(function () {


                                $rootScope.Prompt('Multiple', 'No Of Pieces', function (count) {

                                    var totalcount = $rootScope.GetNumber(count);
                                    if (totalcount > 0) {

                                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - $scope.CurrentBill.Billitems[index].qty).round(2);
                                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                                        $scope.CurrentBill.Billitems[index].qty = totalcount;
                                        $scope.CurrentBill.Billitems[index].piece = totalcount;

                                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + $scope.CurrentBill.Billitems[index].qty).round(2);

                                        $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);

                                        $rootScope.Toaster.Notice('Qty altered to ' + $scope.CurrentBill.Billitems[index].piece);
                                        $scope.UpdateOffer();
                                    }
                                }, $scope.CurrentBill.Billitems[index].piece);
                            }, 25);
                        }
                        else {
                            $scope.UpdateOffer();
                        }
                        return;
                    }
                }
            }

            $scope.SalesDetail.printingname = '';
            $scope.SalesDetail.productid = 0;
            $scope.SalesDetail.sellingmode = 0;
            $scope.SalesDetail.barcodeDetail = null;
            $scope.SalesDetail.qtyeditable = false;
            $scope.SalesDetail.priceeditable = false;

            if ($scope.autoCoding.code.startsWith('#') || $scope.BillNoConfig.isbillno($scope.autoCoding.code)
                || $scope.BillNoConfig.isbulkbillno($scope.autoCoding.code)) {
                $scope.BillNoHandling();
                return;
            }

            $scope.SearchBarcodeDirect(false);
        };

        $scope.SearchBarcodeDirect = function (fromBillSearch) {

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return;
            }


            if (typeof ($scope.CurrentBill.isbulkreturn) != 'undefined' &&
                $scope.CurrentBill.isbulkreturn) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }

            if (typeof ($scope.CurrentBill.isbulkreturngoods) != 'undefined' &&
                $scope.CurrentBill.isbulkreturngoods) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }

            var barcode = $scope.autoCoding.code;
            if ($scope.system_maxbarcodelength > 0 && barcode.length > $scope.system_maxbarcodelength) {

                var barcodeparts = barcode.split(/(\d+)/);
                if (barcodeparts[0].length == 1) {
                    barcode = barcode.substring(0,
                        $scope.system_maxbarcodelength + barcodeparts[0].length);
                }
                else {
                    barcode = barcode.substring(0,
                        $scope.system_maxbarcodelength);
                }
            }

            if (barcode == null || barcode == '' || barcode.indexOf(' ') >= 0 ||
                barcode.indexOf('*') >= 0 || barcode.indexOf('/') >= 0 || barcode.indexOf('\\') >= 0) {

                if (!fromBillSearch && !$scope.BillNoHandling())
                    $rootScope.Toaster.Error('Unsupported barcode format');
                return;
            }
            restService.Get($scope.SearchAPI + $rootScope.LocationInfo.location.id + '/' + encodeURIComponent(barcode)).then(function (results) {

                if (results.errormessage != null && results.errormessage != '') {
                    $rootScope.Toaster.Error(results.errormessage);
                }

                if (results.id != 0) {

                    //if ($scope.CurrentBill.id > 0) {
                    //    $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
                    //    $scope.CurrentBill = (JSON.parse(JSON.stringify($scope.TemplateCurrentBill)));
                    //    $scope.SchemeReturnBillItems = [];
                    //    $scope.CurrentBill.billdate = new Date();
                    //}

                    if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                        $scope.currentimageid = results.imageid;
                        $scope.ShowImage = true;
                    }
                    else {
                        $scope.ShowImage = false;
                    }

                    $scope.SalesDetail.barcode = results.barcode;
                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = { 'serialno': results.serialno };

                    //if ($scope.CurrentBill.ccmarginmode) {
                    //    if (results.buyingprice > 0) {
                    //        var newprice = results.buyingprice + (results.buyingprice * $scope.CurrentBill.ccmargin / 100).round(0);
                    //        var newdiscount = results.price - newprice;
                    //        results.discount = 0;
                    //        results.additionaldiscount = 0;
                    //        results.price = newprice;
                    //        results.additionaldiscountpercentage = 0;

                    //        results.taxInfo = $scope.SalesTax.find(function (arritem) {
                    //            return arritem.id == results.taxid;
                    //        });

                    //        if (results.taxInfo != null) {
                    //            if (results.taxInfo.taxsplit == null) {
                    //                results.taxInfo.taxsplit = { 'taxpercentage': results.taxInfo.taxpercentage };
                    //            }
                    //            results.taxInfo.taxsplit.findTax = $scope.findTax;
                    //        }

                    //        if (results.taxInfo != null && results.taxInfo.taxsplit != null) {
                    //            var taxitem = results.taxInfo.taxsplit.findTax(results.price);
                    //            results.taxpercentage = taxitem.cgst + taxitem.sgst;
                    //        }

                    //        if (!$scope.CurrentBill.isinterstatesale) {
                    //            var halfTax = (results.price * results.taxpercentage / 2 / 100).round(5);
                    //            results.itemtax = (halfTax * 2).round(5);
                    //        }
                    //        else {
                    //            results.itemtax = (results.price * results.taxpercentage / 100).round(5);
                    //        }
                    //        results.price = (results.price + results.itemtax).round(0);
                    //        results.salerate = results.price;
                    //    }
                    //}

                    $scope.AddBarcode(results, false);

                    var product = $scope.Products.find(function (item) {
                        return item.id == results.productid;
                    });

                    $scope.SalesDetail.sellingmode = product.sellingmode;

                    if ($scope.SalesDetail.sellingmode == 3 && !results.stockisbit) {
                        $scope.setFocusautocodingqty();
                    }
                    else {
                        $scope.AddBillItem();
                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.setFocusautocoding();
                    }
                }
                else if (!fromBillSearch) {

                    $scope.BillNoHandling();
                }
            });
        }

        $scope.ShowBatchList = function () {

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return false;
            }

            var httpurl = 'SalablegoodsService/BatchListByBrand/' + $rootScope.LocationInfo.location.id + '/' + $scope.SearchCondition.BrandID.toString();
            if ($scope.IsSaleReturn) {
                httpurl += '/' + $scope.CurrentBill.customerid + '/true';
            }
            else {
                httpurl += '/' + $scope.CurrentBill.customerid + '/false';
            }
            restService.Get(httpurl).then(function (results) {
                $.each(results, function (k, v) {
                    var buyingprice = v.items[0].price;
                    $.each(v.items, function (kk, vv) {
                        vv.buyingprice = buyingprice;
                    });
                });
                $scope.BatchList = results;
                $timeout(function () {
                    $scope.setFocusBatchID();
                    $scope.$digest();
                }, 10);
            });
        }

        $scope.marginchanged = function () {
            var margin = $scope.GetNumber($scope.SalesDetail.margin);
            var pricebased = false;
            if ($scope.SalesDetail.margin && $scope.SalesDetail.margin.toString().startsWith('~')) {
                margin = $scope.GetNumber($scope.SalesDetail.margin.toString().substring(1));
                pricebased = true;
            }
            if ($scope.system_supportpriceedit || pricebased) {
                $scope.SalesDetail.etprice = margin;
                $scope.SalesDetail.etdiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                var additionaldiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                if ($scope.system_b2bexclusivetax) {
                    $scope.SalesDetail.salerate = $scope.SalesDetail.etprice;
                    if (additionaldiscountpercentage > 0) {
                        $scope.SalesDetail.etdiscount = ($scope.SalesDetail.etprice * additionaldiscountpercentage / 100).round(2);
                        $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.etprice * additionaldiscountpercentage / 100).round(2);
                    }
                    if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null) {
                        $scope.SalesDetail.taxpercentage = $scope.SalesDetail.taxInfo.taxpercentage;
                        $scope.SalesDetail.taxname = $scope.SalesDetail.taxInfo.name;
                        if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                            $scope.SalesDetail.additionaldiscount = 0;

                        if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount))
                            $scope.SalesDetail.schemediscount = 0;

                        $scope.SalesDetail.baseprice = ($scope.SalesDetail.etprice - $scope.SalesDetail.additionaldiscount);
                        if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                            var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.baseprice);
                            $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                        }

                        if (!$scope.CurrentBill.isinterstatesale) {
                            var halfTax = ($scope.SalesDetail.etprice * ($scope.SalesDetail.taxpercentage / 2) / 100).round(5);
                            $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                        }
                        else {
                            $scope.SalesDetail.itemtax = ($scope.SalesDetail.etprice * $scope.SalesDetail.taxpercentage / 100).round(5);
                        }

                        $scope.SalesDetail.price = ($scope.SalesDetail.etprice + $scope.SalesDetail.itemtax).round(2);
                        $scope.SalesDetail.salerate = ($scope.SalesDetail.etprice + $scope.SalesDetail.itemtax).round(5);
                        $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(5);
                        $scope.SalesDetail.amount = (($scope.SalesDetail.salerate * $scope.SalesDetail.qty)).round(2);
                    }
                }
                else {
                    $scope.SalesDetail.salerate = $scope.SalesDetail.etprice;
                    $scope.SalesDetail.price = $scope.SalesDetail.etprice;
                    if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null) {
                        $scope.SalesDetail.taxpercentage = $scope.SalesDetail.taxInfo.taxpercentage;
                        $scope.SalesDetail.taxname = $scope.SalesDetail.taxInfo.name;
                        if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                            $scope.SalesDetail.additionaldiscount = 0;

                        if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount))
                            $scope.SalesDetail.schemediscount = 0;

                        $scope.SalesDetail.baseprice = $scope.SalesDetail.etprice - $scope.GetNumber($scope.SalesDetail.etdiscount);
                        if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                            var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.baseprice);
                            $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                        }

                        if (!$scope.CurrentBill.isinterstatesale) {
                            var halfTax = ($scope.SalesDetail.price * ($scope.SalesDetail.taxpercentage / 2) / 100).round(5);
                            $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                        }
                        else {
                            $scope.SalesDetail.itemtax = ($scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 100).round(5);
                        }
                        $scope.SalesDetail.etprice = $scope.SalesDetail.price - $scope.SalesDetail.itemtax;
                        $scope.SalesDetail.salerate = ($scope.SalesDetail.price).round(2);
                        $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(5);
                        $scope.SalesDetail.amount = (($scope.SalesDetail.salerate * $scope.SalesDetail.qty)).round(2);
                    }
                }
            }
            else {
                if ($scope.SalesDetail.barcodeDetail == undefined)
                    $scope.SalesDetail.barcodeDetail = {};
                $scope.SalesDetail.barcodeDetail.buyingprice = $scope.SalesDetail.cost;

                $scope.SalesDetail.price = $scope.SalesDetail.cost;
                $scope.SalesDetail.amount = $scope.SalesDetail.cost * $scope.GetNumber($scope.SalesDetail.qty);

                if (margin > 0 && !pricebased) {
                    $scope.SalesDetail.price += ($scope.SalesDetail.price * margin / 100).round(2)
                }
                else {
                    $scope.SalesDetail.price = margin;
                }

                if ($scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) > 0) {
                    $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.price * $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) / 100).round(2)
                    $scope.SalesDetail.price -= $scope.SalesDetail.additionaldiscount;
                    //$scope.SalesDetail.amount -= ($scope.SalesDetail.additionaldiscount * $scope.GetNumber($scope.SalesDetail.qty)).round(2);
                }

                $scope.SalesDetail.price = parseFloat($scope.SalesDetail.price).round(4);
                $scope.SalesDetail.rate = $scope.SalesDetail.price;

                if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null) {
                    $scope.SalesDetail.taxpercentage = $scope.SalesDetail.taxInfo.taxpercentage;
                    $scope.SalesDetail.taxname = $scope.SalesDetail.taxInfo.name;
                    if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                        $scope.SalesDetail.additionaldiscount = 0;

                    if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount))
                        $scope.SalesDetail.schemediscount = 0;

                    $scope.SalesDetail.baseprice = $scope.SalesDetail.price - $scope.GetNumber($scope.SalesDetail.etdiscount);
                    if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.baseprice);
                        $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                    }

                    if (!$scope.CurrentBill.isinterstatesale) {
                        var halfTax = ($scope.SalesDetail.price * ($scope.SalesDetail.taxpercentage / 2) / 100).round(5);
                        $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                    }
                    else {
                        $scope.SalesDetail.itemtax = ($scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 100).round(5);
                    }

                    $scope.SalesDetail.price = ($scope.SalesDetail.price + $scope.SalesDetail.itemtax).round(2);
                    $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(5);
                    $scope.SalesDetail.amount = (($scope.SalesDetail.price * $scope.SalesDetail.qty)).round(2);
                    //$scope.SalesDetail.amount = ($scope.SalesDetail.amount + $scope.SalesDetail.tax).round(0);
                }

                $scope.SalesDetail.rate = $scope.SalesDetail.price - $scope.SalesDetail.itemtax;
                $scope.SalesDetail.saleamount = $scope.SalesDetail.rate * $scope.SalesDetail.qty;
            }
        }

        $scope.AddB2BItem = function () {

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return false;
            }

            var selectedbatchid = $scope.SearchCondition.BatchID;
            var selectedbatch = $scope.BatchList.find(function (item) {
                return item.id == selectedbatchid;
            });

            var baseSalesDetail = (JSON.parse(JSON.stringify($scope.SalesDetail)));
            var margin = $scope.GetNumber($scope.SalesDetail.margin);
            var pricebased = false;
            if (baseSalesDetail.margin && baseSalesDetail.margin.toString().startsWith('~')) {
                baseSalesDetail.margin = baseSalesDetail.margin.toString().substring(1);
                pricebased = true;
            }

            var require = $scope.GetNumber($scope.SalesDetail.qty);
            if ($scope.sale_negativestock && !$scope.IsSaleReturn && selectedbatch.qty < require) {
                $rootScope.Toaster.Notice('Available stock - ' + selectedbatch.qty);
                return;
            }

            $scope.CurrentBill.ccmargin = baseSalesDetail.margin;
            $scope.CurrentBill.ccmarginmode = true;
            if (require < 0 || $scope.IsSaleReturn) {
                require = Math.abs(require);
                for (var i = 0; require > 0 && i < selectedbatch.items.length; i++) {
                    var results = selectedbatch.items[i];

                    $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                    $scope.SalesDetail.barcode = results.barcode;
                    $scope.SalesDetail.stock = results.oqty;
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = { 'serialno': results.serialno };
                    $scope.SalesDetail.additionaldiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                    results.price = $scope.SalesDetail.price;
                    results.discount = 0;
                    results.discountpercentage = 0;
                    results.additionaldiscount = $scope.SalesDetail.additionaldiscount;
                    results.additionaldiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                    results.schemediscount = 0;
                    results.schemediscountpercentage = 0;
                    $scope.AddBarcode(results, false);

                    var product = $scope.Products.find(function (item) {
                        return item.id == results.productid;
                    });

                    $scope.SalesDetail.sellingmode = product.sellingmode;
                    $scope.SalesDetail.qty = 0;

                    if (results.iqty > require) {
                        $scope.SalesDetail.qty = require;
                        require = 0;
                    }
                    else {
                        $scope.SalesDetail.qty = results.iqty;
                        require -= $scope.SalesDetail.qty;
                    }

                    $scope.SalesDetail.qty = $scope.SalesDetail.qty * -1;
                    $scope.AddBillItem(true);
                }
            }
            else {
                for (var i = 0; require > 0 && i < selectedbatch.items.length; i++) {
                    var results = selectedbatch.items[i];

                    $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                    $scope.SalesDetail.barcode = results.barcode;
                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = { 'serialno': results.serialno };
                    $scope.SalesDetail.additionaldiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                    results.price = $scope.SalesDetail.price;
                    results.discount = 0;
                    results.discountpercentage = 0;
                    results.additionaldiscount = $scope.SalesDetail.additionaldiscount;
                    results.additionaldiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                    results.schemediscount = 0;
                    results.schemediscountpercentage = 0;
                    $scope.AddBarcode(results, false);

                    var product = $scope.Products.find(function (item) {
                        return item.id == results.productid;
                    });

                    $scope.SalesDetail.sellingmode = product.sellingmode;
                    $scope.SalesDetail.qty = 0;

                    if (results.iqty - (results.dqty + results.oqty) > require) {
                        $scope.SalesDetail.qty = require;
                        require = 0;
                    }
                    else {
                        $scope.SalesDetail.qty = results.iqty - (results.dqty + results.oqty);
                        require -= results.iqty - (results.dqty + results.oqty);
                    }
                    $scope.AddBillItem(true, pricebased);
                }
            }
            if (require > 0) {
                $rootScope.Toaster.Notice('Stock Shortage - ' + require);
            }
            $scope.SearchCondition.BatchID = null;
            $scope.setFocusBatchID();
        }

        $scope.SelectBatch = function () {
            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid <= 0 ||
                typeof ($scope.CurrentBill.customerdiscount) == 'undefined' || $scope.CurrentBill.customerdiscount <= 0) {
            }

            if (typeof ($scope.SearchCondition.BatchID) == 'undefined' ||
                $scope.SearchCondition.BatchID == null ||
                $scope.SearchCondition.BatchID == '') {
                $rootScope.Toaster.Notice('Please select the batch');
                return;
            }


            var selectedbatchid = $scope.SearchCondition.BatchID;
            var selectedbatch = $scope.BatchList.find(function (item) {
                return item.id == selectedbatchid;
            });

            if (typeof (selectedbatch) != 'undefined' && selectedbatch != null) {
                $scope.SelectedBatch = selectedbatch;
            }
            else {
                return;
            }

            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));

            //var qty = $scope.GetNumber($scope.SearchCondition.qty);
            //if (qty <= 0) {
            //    $rootScope.Toaster.Notice('Please enter the qty');
            //    return;
            //}

            //if (selectedbatch.qty < qty) {
            //    $rootScope.Toaster.Notice('Available Stock - ' + selectedbatch.qty);
            //    return;
            //}

            var selectedbrand = $scope.Brands.find(function (item) {
                return item.id == $scope.SearchCondition.BrandID;
            });

            $scope.SalesDetail.designid = selectedbatch.items[0].designid;
            $scope.SalesDetail.cost = selectedbatch.items[0].buyingprice;
            if (typeof (selectedbrand) != 'undefined') {
                $scope.SalesDetail.brand = selectedbrand.name;
                $scope.SalesDetail.size = selectedbatch.items[0].sizename;
            }

            if (typeof ($scope.CurrentBill.ccmarginmode) != 'undefined') {
                $scope.SalesDetail.margin = $scope.CurrentBill.ccmargin;
            }
            //var require = qty;
            //for (var i = 0; require > 0 && i < selectedbatch.items.length; i++) {
            //    var item = selectedbatch.items[i];
            //    if (item.iqty - (item.dqty + item.oqty) > require) {
            //        $scope.ShowBatchDetail(item, require);
            //        require = 0;
            //    }
            //    else {
            //        $scope.ShowBatchDetail(item, item.iqty - (item.dqty + item.oqty));
            //        require -= item.iqty - (item.dqty + item.oqty);
            //    }
            //}

            if (typeof (selectedbatch.items[0].imageid) != 'undefined' && selectedbatch.items[0].imageid != null && selectedbatch.items[0].imageid.length > 0) {
                $scope.currentimageid = selectedbatch.items[0].imageid;
                $scope.ShowImage = true;
            }
            else {
                $scope.ShowImage = false;
            }

            $scope.SalesDetail.barcode = selectedbatch.items[0].barcode;
            $scope.SalesDetail.stock = (selectedbatch.items[0].iqty - (selectedbatch.items[0].dqty + selectedbatch.items[0].oqty));
            $scope.SalesDetail.hsncode = selectedbatch.items[0].hsncode;
            $scope.SalesDetail.attributes = { 'serialno': selectedbatch.items[0].serialno };

            var product = $scope.Products.find(function (item) {
                return item.id == selectedbatch.items[0].productid;
            });

            $scope.SalesDetail.sellingmode = product.sellingmode;
            $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                return arritem.id == selectedbatch.items[0].taxid;
            });

            if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null)
                $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

            $scope.SearchCondition.qty = null;

            if ($scope.GetNumber($scope.SalesDetail.margin) == 0)
                $scope.SalesDetail.margin = null;

            $timeout(function () {
                $scope.setFocusmargin();
                $scope.$digest();
            }, 10);
        }

        $scope.ShowBatchDetail = function (results, qty) {
            if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                $scope.currentimageid = results.imageid;
                $scope.ShowImage = true;
            }
            else {
                $scope.ShowImage = false;
            }

            $scope.SalesDetail.barcode = results.barcode;
            $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
            $scope.SalesDetail.hsncode = results.hsncode;
            $scope.SalesDetail.attributes = { 'serialno': results.serialno };
            $scope.AddBarcode(results, false);

            var product = $scope.Products.find(function (item) {
                return item.id == results.productid;
            });

            $scope.SalesDetail.sellingmode = product.sellingmode;
            $scope.SalesDetail.qty = qty;
            $scope.AddBillItem();
        }

        $scope.BillNoHandling = function () {
            //($scope.billprefix != null && $scope.autoCoding.code.startsWith($scope.billprefix) && $scope.autoCoding.code.length > 8) || ($scope.autoCoding.code.startsWith('#'))
            if ($scope.BillNoConfig.isbillno($scope.autoCoding.code)) {
                $scope.SearchCondition.billno = ($scope.autoCoding.code.startsWith('#') ? $scope.autoCoding.code.substring(1) : $scope.autoCoding.code);
                $scope.SearchCondition.billid = 0;
                $scope.SearchCondition.isbulkreturn = false;
                $scope.SearchBill(function (bill) {

                    if (bill != null) {
                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.CurrentBill.isbulkreturn = false;
                        $scope.CurrentBill.returnbillno = bill.billno;
                        bill.returnbillno = bill.billno;
                        var oldbill = $scope.lastBills.find(function (item) {
                            return item.id == bill.id;
                        });

                        if (typeof (oldbill) == 'undefined' || oldbill == null) {
                            $scope.lastBills.splice(0, 0, (JSON.parse(JSON.stringify(bill))));
                        }
                        $scope.setFocusautocoding();
                    }
                    else {
                        $scope.SearchBarcodeDirect(true);
                    }

                });
                return true;
            } else if (!$scope.IsSaleReturn && $scope.BillNoConfig.isbulkbillno($scope.autoCoding.code)) {
                $scope.SearchCondition.billno = $scope.autoCoding.code;
                $scope.SearchCondition.billid = 0;
                $scope.SearchCondition.isbulkreturn = true;
                $scope.SearchBill(function (bill) {

                    if (bill != null) {

                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.CurrentBill.isbulkreturn = true;
                        $scope.CurrentBill.returnbillno = bill.billno;
                        $scope.CurrentBill.billno = bill.billno;
                        bill.returnbillno = bill.billno;
                        bill.isbulkreturn = true;

                        $scope.setFocusautocoding();

                        var oldbill = $scope.lastBills.find(function (item) {
                            return item.id == bill.id;
                        });

                        if (typeof (oldbill) == 'undefined' || oldbill == null) {
                            $scope.lastBills.splice(0, 0, (JSON.parse(JSON.stringify(bill))));
                        }

                        if ($scope.IsBulkSale) {
                            $rootScope.Confirm('Return goods', 'Do you want to return the previous approval goods without billing?',
                                function () {
                                    if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                        $scope.CurrentBill.billattributes = {};
                                    $scope.CurrentBill.isbulkreturngoods = true;
                                });
                        } else {
                            $rootScope.Confirm('Return goods', 'Did you received products except billing products?',
                                function () {
                                    if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                        $scope.CurrentBill.billattributes = {};
                                    $scope.CurrentBill.isbulkreturngoods = true;
                                });
                        }

                    } else {
                        $scope.SearchBarcodeDirect(true);
                    }
                    /*if ($scope.IsBulkSale) {
                        $scope.ShowCancellDialog(bill);
                    };*/
                });
                return true;
            }
        }

        $scope.CreateNew();

        $scope.RequestCustomerOTP = function () {
            var request = {
                "Item": {
                    'id': $scope.Parent.CurrentBill.customerid,
                    'mobileno': $scope.Parent.CurrentBill.phoneno
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CustomerService/SendOTP",
                data: srequest
            }).success(function (result) {
                $rootScope.Prompt('OTP Approval', 'Enter Customer OTP', function (value) {
                    $scope.ConfirmOTP(value);
                });
            });
        }

        $scope.ConfirmOTP = function (otp) {

            if (typeof (otp) == 'undefined' || otp == '' || otp.length < 4) {
                $rootScope.Toaster.Notice('Customer discount disabled');
                $scope.Parent.CurrentBill.discountotpconfirmed = false;
                return;
            }

            var request = {
                "Item": {
                    'id': $scope.Parent.CurrentBill.customerid,
                    'mobileno': $scope.Parent.CurrentBill.phoneno,
                    'otp': otp
                },
                "SecurityToken": "String content"
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CustomerService/ConfirmOTP",
                data: srequest
            }).success(function (result) {

                if (result.Success && result.Item) {
                    $scope.Parent.CurrentBill.discountotpconfirmed = true;
                    $scope.ApplyCustomerDiscount();
                }
                else {
                    $rootScope.Toaster.Notice(result.ErrorMessage == null ? 'OTP Mismatch' : result.ErrorMessage);
                    $rootScope.Prompt('OTP Approval', 'Enter Customer OTP', function (value) {
                        $scope.ConfirmOTP(value);
                    });
                }
            });
        }


        $scope.CheckCreditFocus = function () {
            if (!$scope.CurrentBill.iscreditbill &&
                $scope.CurrentBill.customername != null &&
                $scope.CurrentBill.customername.length > 0) {
                $scope.setFocusautocoding();
            }
            else {
                $scope.setFocuscustomername();
            }
        }

        $scope.LastBiteEnter = function () {
            var islastbit = false;
            var ismultiqty = false;
            var multiqty = 0;
            $scope.autoCoding.lastbit = '';
            if ($scope.autoCoding.qty.toString().indexOf('-') > 0) {
                var bitinfo = $scope.autoCoding.qty.split('-');
                var qty = $scope.GetNumber(bitinfo[0]);
                var lastbit = $scope.GetNumber(bitinfo[1]);
                if (qty < lastbit) {
                    $rootScope.Toaster.Error('last bit discount cant exceed actual');
                    return;
                }
                var journalqty = qty - lastbit;
                $scope.autoCoding.lastbit = journalqty.toString();
                $scope.autoCoding.qty = qty.toString();
                islastbit = true;
            } else if ($scope.autoCoding.qty.toString().indexOf('*') > 0) {
                ismultiqty = true;
                var bitinfo = $scope.autoCoding.qty.split('*');
                var qty = $scope.GetNumber(bitinfo[0]);
                multiqty = $scope.GetNumber(bitinfo[1]);
                $scope.autoCoding.qty = qty.toString();
            }

            var bqty = Math.abs($scope.GetNumber($scope.autoCoding.qty));
            $scope.SalesDetail.qty = bqty;

            if (!ismultiqty) {
                $scope.AddBillItem();
            }
            else {
                var idstart = new Date().getTime();
                var saledetailcopy = (JSON.parse(JSON.stringify($scope.SalesDetail)));
                for (var i = 0; i < multiqty; i++) {
                    $scope.SalesDetail = (JSON.parse(JSON.stringify(saledetailcopy)));
                    $scope.SalesDetail.id = idstart + i;
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                    $scope.AddBillItem();
                }
            }

            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.lastbit = "";
            if (islastbit)
                $rootScope.Toaster.Notice('last bit added');
            $scope.setFocusautocoding();
        };

        hotkeys.add('alt+p', 'Print', function (event) {
            event.preventDefault();
            $scope.Print($scope.CurrentBill);
        });

        $scope.ApplyRewardDialog = function () {
            var dialog = ngDialog.open({
                template: 'ApplyReward',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.Parent = $scope.ngDialogData[0]
                    $scope.reward_point = 0;
                    $scope.reward_convertable = 0;
                    $scope.reward_amount = 0;
                    $scope.couponcode = '';
                    $scope.coupon_convertable = 0;
                    $scope.coupon_amount = 0;
                    $scope.coupon_value = 0;
                    $scope.reason = $scope.Parent.ApplicationSettings.appsettings.sale_customerOTP ? 'Require OTP to Claim Rewards.' : '';
                    $scope.couponid = 0;
                    $scope.reward_maxamount = 0;
                    $scope.coupon_maxamount = 0;
                    $scope.showmanual = $scope.Parent.ManualRewards;
                    $scope.ismanual = false;
                    $scope.manual_value = 0;
                    $scope.manual_amount = 0;
                    if (!$scope.showmanual) {
                        $timeout(function () {
                            $scope.setFocusapplyreward();
                            $scope.$digest();
                        }, 10);
                    }

                    $scope.Clear = function () {
                        $scope.closeThisDialog({ 'type': 'clear', 'amount': 0 });
                    }

                    $scope.ApplyReward = function ($event) {


                        if ($scope.reward_amount < 0 || ($scope.reward_amount > $scope.reward_maxamount && $scope.reward_maxamount > 0)) {
                            $rootScope.Toaster.Notice('Amount cant exceed ' + $scope.reward_maxamount);
                            return;
                        }

                        var requiredpoints = ($scope.reward_point / $scope.reward_convertable) * $scope.reward_amount;
                        if (requiredpoints < ($scope.reward_point - $scope.reward_new))
                            $scope.reward_new = 0;
                        else
                            $scope.reward_new = requiredpoints - ($scope.reward_point - $scope.reward_new);

                        if ($scope.Parent.ApplicationSettings.appsettings.sale_customerOTP) {
                            var request = {
                                "Item": {
                                    'id': $scope.Parent.CurrentBill.customerid,
                                    'mobileno': $scope.Parent.CurrentBill.phoneno
                                },
                                "SecurityToken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "CustomerService/SendOTP",
                                data: srequest
                            }).success(function (result) {
                                $rootScope.Prompt('OTP Approval', 'Enter Customer OTP', function (value) {
                                    $scope.ConfirmOTP(value);
                                });
                            });
                        }
                        else {
                            $scope.closeThisDialog({
                                'type': 'reward', 'amount': $scope.reward_amount, 'newreward': $scope.reward_new,
                                'reward': requiredpoints
                            });
                        }
                    }

                    $scope.ConfirmOTP = function (otp) {
                        var request = {
                            "Item": {
                                'id': $scope.Parent.CurrentBill.customerid,
                                'mobileno': $scope.Parent.CurrentBill.phoneno,
                                'otp': otp
                            },
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/ConfirmOTP",
                            data: srequest
                        }).success(function (result) {
                            var requiredpoints = ($scope.reward_point / $scope.reward_convertable) * $scope.reward_amount;
                            if (result.Success && result.Item)
                                $scope.closeThisDialog({ 'type': 'reward', 'amount': $scope.reward_amount, 'newreward': $scope.reward_new, 'reward': requiredpoints });
                            else
                                $rootScope.Toaster.Notice(result.ErrorMessage == null ? 'OTP Mismatch' : result.ErrorMessage);
                        });
                    }

                    $scope.ApplyCoupon = function () {
                        if ($scope.coupon_amount < 0 || ($scope.coupon_amount > $scope.coupon_maxamount && $scope.coupon_maxamount > 0)) {
                            $rootScope.Toaster.Notice('Amount cant exceed ' + $scope.coupon_maxamount);
                            return;
                        }
                        $scope.closeThisDialog({
                            'type': 'coupon', 'amount': $scope.coupon_amount,
                            'coupon_code': $scope.couponcode, 'coupon_id': $scope.couponid
                        });
                    }

                    $scope.ApplyManual = function () {
                        if ($scope.manual_point - $scope.reward_new < $scope.manual_value)
                            $scope.reward_new = $scope.manual_value - ($scope.manual_point - $scope.reward_new);
                        else
                            $scope.reward_new = 0;
                        $scope.closeThisDialog({
                            'type': 'manual', 'amount': $scope.manual_amount,
                            'rewards': $scope.manual_value, 'newreward': $scope.reward_new
                        });
                    }


                    $scope.findreward = function () {

                        $scope.receivable = $scope.Parent.CurrentBill.receivable;
                        var srequest = JSON.stringify({
                            "Item": {
                                'customerid': $scope.Parent.CurrentBill.customerid,
                                'locationid': $rootScope.LocationInfo.location.id,
                                'billvalue': $scope.Parent.CurrentBill.receivable
                            },
                            "securitytoken": "String content"
                        });

                        $http({
                            method: 'POST',
                            url: "CustomerrewardsService/SearchReward",
                            data: srequest
                        }).success(function (result) {
                            if (result != null && typeof (result) != 'undefined') {
                                $scope.reward_point = result.rewards;
                                $scope.reward_convertable = result.available;
                                $scope.reward_amount = result.convertable;
                                $scope.reward_maxamount = result.convertable;
                                $scope.manual_point = result.rewards;
                                $scope.reward_new = result.billrewards;
                                if ($scope.showmanual)
                                    $scope.setFocusmanual_point();
                            }
                        }).error(function (result) {

                        });
                    }();

                    $scope.FindCoupon = function () {
                        var srequest = JSON.stringify({
                            "Item": {
                                'acceptablelocationid': $rootScope.LocationInfo.location.id,
                                'code': $scope.couponcode,
                                'billvalue': $scope.Parent.CurrentBill.receivable,
                                'bill': $scope.Parent.CurrentBill,
                            },
                            "securitytoken": "String content"
                        });

                        $http({
                            method: 'POST',
                            url: "VouchersService/SearchCoupen",
                            data: srequest
                        }).success(function (result) {
                            if (result != null && typeof (result) != 'undefined') {
                                $scope.coupon_value = result.available;
                                $scope.coupon_convertable = result.convertable;
                                $scope.coupon_amount = result.convertable;
                                $scope.coupon_maxamount = result.convertable;

                                $scope.reason = result.reason;
                                $scope.couponid = result.couponid;
                                $scope.setFocusapplycoupon();
                            }
                        }).error(function (result) {

                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        data.value.amount = parseFloat(data.value.amount);
                        $scope.ApplyAdditionalDiscount(parseFloat(data.value.amount));
                        $scope.CurrentBill.HasCouponApplied = false;
                        if (data.value.amount == 0) {
                            $scope.CurrentBill.billattributes = {};
                        }
                        else {
                            $scope.CurrentBill.billattributes = {};
                            $scope.CurrentBill.billattributes.mode = data.value.type;
                            $scope.CurrentBill.billattributes.discountamount = data.value.amount;
                            if (data.value.type == 'reward') {
                                $scope.CurrentBill.billattributes.rewardamount = data.value.amount;
                                $scope.CurrentBill.billattributes.newreward = data.value.newreward;
                            }
                            else if (data.value.type == 'coupon') {
                                $scope.CurrentBill.billattributes.couponamount = data.value.amount;
                                $scope.CurrentBill.billattributes.couponid = data.value.coupon_id;
                                $scope.CurrentBill.billattributes.couponcode = data.value.coupon_code;
                                $scope.CurrentBill.HasCouponApplied = $scope.CurrentBill.billattributes.couponamount > 0;
                            }
                            else if (data.value.type == 'manual') {
                                $scope.CurrentBill.billattributes.manualrewardamount = data.value.amount;
                                $scope.CurrentBill.billattributes.manualrewardpoint = data.value.rewards;
                                $scope.CurrentBill.billattributes.newreward = data.value.newreward;
                            }
                        }
                    }

                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                }
            });
        };

        $scope.ApplySingleRowDiscount = function (item, type, newvalue) {
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;

            item.receivable += (item.additionaldiscount * item.qty);
            if (type == '%') {
                item.additionaldiscount = (item.receivable * newvalue / 100 / item.qty).round(2);
                item.additionaldiscountpercentage = newvalue.round(2);
            }
            else {
                item.additionaldiscount = (newvalue / item.qty).round(2);
                item.additionaldiscountpercentage = (((item.additionaldiscount * item.qty) / (item.receivable + item.discount)) * 100).round(2);
            }

            item.receivable -= (item.additionaldiscount * item.qty).round(2);

            $.each($scope.CurrentBill.Billitems, function (k, v) {
                v.additionaldiscount = $scope.GetNumber(v.additionaldiscount);
                v.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage);

                if (typeof (v.barcodeDetail) == 'undefined') {
                    v.barcodeDetail = {
                        'taxid': v.taxid,
                        'discount': (v.discount / v.qty),
                        'additionaldiscount': v.additionaldiscount,
                        'stockid': v.stockid,
                        'id': v.salablegoodsid,
                        'companyid': v.companyid
                    };

                    if (v.taxid > 0) {
                        v.taxInfo = $scope.SalesTax.find(function (arritem) {
                            return arritem.id == v.taxid;
                        });

                        if (v.taxInfo != null)
                            v.taxInfo.taxsplit.findTax = $scope.findTax;
                    }
                    v.barcodeDetail.discount = v.discount;
                    v.barcodeDetail.additionaldiscount = 0;
                }
                $scope.UpdateTax(v);
                $scope.UpdateItemInfo(v, false);
            });
        }

        $scope.ApplyAdditionalDiscount = function (newvalue) {

            $scope.PopupList = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                $scope.PopupList.push({
                    'core': v, 'additionaldiscount': v.additionaldiscount, 'additionaldiscountpercentage': v.additionaldiscountpercentage, 'receivable': v.receivable
                });
            });

            var avail = newvalue;
            $scope.TotalValue = 0;
            $.each($scope.PopupList, function (k, v) {
                $scope.TotalValue += v.core.receivable;
            });

            var percent = (avail / $scope.TotalValue * 100).round(2);
            for (var index = 0; index < $scope.PopupList.length; index++) {
                if (index == ($scope.PopupList.length - 1)) {
                    $scope.PopupList[index].additionaldiscount = (avail / $scope.PopupList[index].core.qty).round(2);
                    $scope.PopupList[index].additionaldiscountpercentage = ((($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty) / ($scope.PopupList[index].core.receivable + $scope.PopupList[index].core.discount)) * 100).round(2);
                }
                else {
                    $scope.PopupList[index].additionaldiscount = ($scope.PopupList[index].core.receivable * percent / 100 / $scope.PopupList[index].core.qty).round(2);
                    $scope.PopupList[index].additionaldiscountpercentage = ((($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty) / ($scope.PopupList[index].core.receivable + $scope.PopupList[index].core.discount)) * 100).round(2);
                    avail -= ($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty);
                }

                $scope.PopupList[index].receivable += ($scope.PopupList[index].core.additionaldiscount * $scope.PopupList[index].core.qty)
                $scope.PopupList[index].receivable -= ($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty);
                $scope.PopupList[index].receivable = $scope.PopupList[index].receivable.round(2);
            }

            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            $.each($scope.PopupList, function (k, v) {
                v.core.additionaldiscount = $scope.GetNumber(v.additionaldiscount);
                v.core.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage);

                if (typeof (v.core.barcodeDetail) == 'undefined') {
                    v.core.barcodeDetail = { 'taxid': v.core.taxid };

                    if (v.core.taxid > 0) {
                        v.core.taxInfo = $scope.SalesTax.find(function (arritem) {
                            return arritem.id == v.core.taxid;
                        });

                        if (v.core.taxInfo != null)
                            v.core.taxInfo.taxsplit.findTax = $scope.findTax;
                    }
                    v.core.barcodeDetail.discount = v.core.discount;
                    v.core.barcodeDetail.additionaldiscount = 0;
                }
                $scope.UpdateTax(v.core);
                $scope.UpdateItemInfo(v.core, false);
            });
            $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
        }

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', function ($scope, $timeout, $http, hotkeys, $rootScope) {

                    $scope.PopupList = [];
                    $scope.Customer = null;
                    $scope.CloseIfOne = false;

                    $scope.Select = function () {
                        $scope.Customer = null;
                        $.each($scope.PopupList, function (k, v) {
                            if (v.selected)
                                $scope.Customer = v;
                        });
                        if ($scope.Customer != null)
                            $scope.closeThisDialog($scope.Customer);
                    };
                    $scope.Create = function () {
                        if (!$scope.name || $scope.name == null || $scope.name == '') {
                            $rootScope.Toaster.Notice('Please enter the customer name');
                        }
                        if (!$scope.contactno || $scope.contactno == null || $scope.contactno == '') {
                            $rootScope.Toaster.Notice('Please enter the customer contact no');
                        }
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'registredlocationid': $rootScope.LocationInfo.location.id,
                                'locationid': $rootScope.LocationInfo.location.id, 'customertype': 2
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }

                    $scope.Search = function (closeIfOne) {

                        $scope.CloseIfOne = typeof (closeIfOne) != 'undefined' ? closeIfOne : false;
                        if (!$scope.hasvalue($scope.name) &&
                            !$scope.hasvalue($scope.area) &&
                            !$scope.hasvalue($scope.contactno) &&
                            !$scope.hasvalue($scope.gstno) &&
                            !$scope.hasvalue($scope.loyeltyno))
                            return;

                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'locationid': $rootScope.LocationInfo.location.id,
                                'registredlocationid': $rootScope.LocationInfo.location.id
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length == 1 && $scope.CloseIfOne) {
                                $scope.closeThisDialog(result[0]);
                            }
                            $scope.PopupList = result;
                        });
                    };

                    $timeout(function () {
                        //$scope.setcustomer_contact();
                    }, 10);
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {

                        var existingCustomer = $scope.Customers.find(function (item) {
                            return item.id == data.value.id;
                        });
                        if (typeof (existingCustomer) == 'undefined' || existingCustomer == null)
                            $scope.Customers.push(data.value);

                        $scope.CurrentBill.customerid = data.value.id;
                        $scope.CurrentBill.code = '';
                        $scope.CurrentBill.phoneno = data.value.mobileno;
                        $scope.CurrentBill.customername = data.value.name;
                        $scope.CurrentBill.customergst = data.value.gstno;
                        $scope.CurrentBill.customerdiscount = data.value.discount;
                        $scope.CurrentBill.ccmargin = data.value.ccmargin;
                        $scope.CurrentBill.ccmarginmode = data.value.ccmarginmode;
                        $scope.CurrentBill.billdiscountinclude = data.value.billdiscountinclude;
                        $scope.CurrentBill.rewardunit = data.value.rewardunit;
                        $scope.CurrentBill.discountotp = data.value.discountotp;

                        if ($scope.CurrentBill.discountotp && ($scope.CurrentBill.ccmarginmode || $scope.CurrentBill.customerdiscount > 0)) {
                            $scope.CurrentBill.discountotpconfirmed = false;
                            $rootScope.Toaster.Notice('Customer OTP Require for special discounts!');
                            $scope.RequestCustomerOTP();
                        }
                        else {
                            $scope.CurrentBill.discountotpconfirmed = false;
                            $scope.CurrentBill.discountotp = false;
                            $scope.ApplyCustomerDiscount();
                        }
                    }
                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                }
            });
        }

        $scope.ShowExchangeDialog = function () {
            $rootScope.Prompt('Exchange', 'Enter Barcode/IMEI', function (value) {

                restService.Get('BuybackService/Barcode/' + $rootScope.LocationInfo.location.id + '/' + encodeURIComponent(value)).then(function (results) {

                    if (results.id != 0) {

                        if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                            $scope.currentimageid = results.imageid;
                            $scope.ShowImage = true;
                        }
                        else {
                            $scope.ShowImage = false;
                        }
                        $scope.SalesDetail.stock = 1;
                        $scope.SalesDetail.qty = -1;
                        $scope.SalesDetail.hsncode = results.hsncode;
                        $scope.SalesDetail.attributes = { 'serialno': results.serialno };
                        $scope.AddBarcode(results);
                        $scope.AddBillItem();
                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.setFocusautocoding();
                    }
                    else {
                        if (results.errormessage != null && results.errormessage != '') {
                            $rootScope.Toaster.Error(results.errormessage);
                        }
                        else {
                            $rootScope.Toaster.Error('Unknown issue, cant find the item');
                        }
                    }

                });

            });
        }

        $scope.ShowEMIDialog = function () {
            if ($scope.CurrentBill.receivable <= 0) return;

            var dialog = ngDialog.open({
                template: 'EMIDialog',
                data: [$scope, $scope.CurrentBill],
                width: '700px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService) {

                        $scope.ParentScop = $scope.ngDialogData[0];
                        $scope.EMIScheme = typeof ($scope.ngDialogData[1].emidetail) == 'undefined' ? {} : $scope.ngDialogData[1].emidetail;
                        $scope.BillValue = $scope.ngDialogData[1].receivable;
                        $scope.EMIScheme.billvalue = $scope.ngDialogData[1].receivable;

                        $scope.SchemeChanged = function () {
                            $scope.CalculatePremium(true);
                        }

                        $scope.CalculatePremium = function (schemechanged) {
                            var scheme = $scope.EMISchemes.find(function (item) {
                                return item.id == $scope.EMIScheme.schemeid;
                            });

                            if (schemechanged && typeof (scheme) != 'undefined' && scheme != null) {
                                $scope.EMIScheme.rateofinterest = scheme.rateofinterest;
                                $scope.EMIScheme.loanproviderid = scheme.loanproviderid;
                                $scope.EMIScheme.schemename = scheme.name;
                                $scope.EMIScheme.tenure = scheme.tenure;
                                var provider = $scope.LoanProviders.find(function (item) {
                                    return item.id == $scope.EMIScheme.loanproviderid;
                                });

                                if (typeof (provider) != 'undefined' && provider != null)
                                    $scope.EMIScheme.loanprovidername = provider.name;

                                if (typeof ($scope.EMIScheme.advancemonth) == 'undefined' ||
                                    $scope.EMIScheme.advancemonth == null || $scope.EMIScheme.advancemonth < scheme.minimumadvancemonth)
                                    $scope.EMIScheme.advancemonth = scheme.minimumadvancemonth;

                                if (scheme.dbdpercentage > 0) {
                                    $scope.EMIScheme.dbdvalue = ($scope.BillValue * scheme.dbdpercentage / 100).round(0);
                                }
                                else {
                                    $scope.EMIScheme.dbdvalue = scheme.dbdvalue;
                                }
                                if (scheme.documentchargepercentage > 0) {
                                    $scope.EMIScheme.documentcharge = ($scope.BillValue * scheme.documentchargepercentage / 100).round(0);
                                }
                                else {
                                    $scope.EMIScheme.documentcharge = scheme.documentcharge;
                                }
                                $scope.EMIScheme.emicardcharge = scheme.emicardcharge;
                            }

                            var marginmoney = $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            var advanceamt = (($scope.BillValue - marginmoney) / $scope.EMIScheme.tenure * $scope.EMIScheme.advancemonth).round(0);
                            advanceamt += $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            var loanAmount = ($scope.BillValue - advanceamt);
                            var numberOfMonths = $scope.EMIScheme.tenure - $scope.EMIScheme.advancemonth;

                            var monthlyInterestRatio = ($scope.EMIScheme.rateofinterest / 100) / 12;
                            var top = Math.pow((1 + monthlyInterestRatio), numberOfMonths);
                            var bottom = top - 1;
                            var sp = top / bottom;
                            var emi = (monthlyInterestRatio <= 0 ? loanAmount / numberOfMonths : ((loanAmount * monthlyInterestRatio) * sp)).round(0);
                            var full = numberOfMonths * emi;
                            var interest = full - loanAmount;
                            var int_pge = (interest / full) * 100;

                            $scope.EMIScheme.loanamount = loanAmount;
                            $scope.EMIScheme.emiamount = emi;
                            $scope.EMIScheme.emicharges = $scope.ParentScop.GetNumber($scope.EMIScheme.dbdvalue)
                                + $scope.ParentScop.GetNumber($scope.EMIScheme.documentcharge)
                                + $scope.ParentScop.GetNumber($scope.EMIScheme.emicardcharge)
                                + $scope.ParentScop.GetNumber($scope.EMIScheme.additionalcharge);
                            - $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            $scope.EMIScheme.advanceemi = advanceamt + $scope.EMIScheme.emicharges;
                        }

                        $scope.EMISchemes = restService.Get('ReferencelistService/EMISCHEMES', true).then(function (results) {
                            $scope.EMISchemes = results;
                        });

                        $scope.LoanProviders = restService.Get('ReferencelistService/LOANPROVIDER', true).then(function (results) {
                            $scope.LoanProviders = results;
                        });

                        $scope.Select = function () {
                            $scope.CalculatePremium();
                            $scope.closeThisDialog($scope.EMIScheme);
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        $scope.CurrentBill.hasemi = true;
                        $scope.CurrentBill.emidetail = data.value;
                        if ($scope.ValidateBill()) {

                            if ($scope.AutoSettlement && (!$scope.CurrentBill.iscreditbill || $scope.system_crsettlementonbill)) {
                                $scope.ShowSettlement($scope.CurrentBill);
                            }
                            else {
                                $scope.SaveBill();
                            }
                        }
                    }
                    else {
                        $scope.autoCoding.code = '';
                        $scope.setFocusautocoding();
                    }
                }
            });
        }


        $scope.TouchButton = function (action, key) {
            $scope.autoCoding.code = key;
            $scope.SearchAutoCoding();
        }

        $scope.ShowAdditionalDiscount = function () {
            var list = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {

                var product = $scope.Products.find(function (item) {
                    return item.id == v.productid;
                });

                if (typeof (product) != 'undefined' && product != null && product.discountmode == 4) {
                    //no bill level discounts.
                }
                else {
                    list.push({
                        'core': v, 'additionaldiscount': v.additionaldiscount, 'additionaldiscountpercentage': v.additionaldiscountpercentage, 'receivable': v.receivable
                    });
                }
            });

            var dialog = ngDialog.open({
                template: 'AdditionalDiscountDialog',
                data: [$scope, list],
                width: '850px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.Receivable = 0;
                    $scope.PopupList = $scope.ngDialogData[1];
                    $scope.ShowPasscode = false;
                    $scope.CanApprove = $rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view;
                    $scope.sale_requestpasscodetosms = $scope.ngDialogData[0].sale_requestpasscodetosms;
                    $scope.system_salesrounding = $scope.ngDialogData[0].system_salesrounding;
                    $scope.rounding_discount = $scope.ngDialogData[0].rounding_discount;
                    $scope.system_b2bexclusivetax = $scope.ngDialogData[0].system_b2bexclusivetax;
                    $scope.CanAskPasscode = !$scope.sale_requestpasscodetosms;
                    $scope.AD = parseFloat($rootScope.UserInfo.AD);
                    if (isNaN($scope.AD)) $scope.AD = 0;

                    $scope.TotalValue = -1;
                    $scope.RoundingValue = 0;
                    $scope.ComputeTotal = function () {
                        $scope.Receivable = 0;
                        $.each($scope.PopupList, function (k, v) {
                            $scope.Receivable += v.receivable;
                        });
                        $scope.RoundingValue = $scope.Receivable;
                        $scope.Receivable = $rootScope.RoundRupee($scope.Receivable, $scope.system_salesrounding);
                        $scope.RoundingValue = $scope.Receivable - $scope.RoundingValue;
                    }

                    $scope.MultipleDiscountCheck = function (item) {
                        if (item.additionaldiscountpercentage.toString().indexOf('+') > 0) {
                            var discountparts = item.additionaldiscountpercentage.toString().split('+');
                            item.additionaldiscount = (item.core.salerate * parseFloat(discountparts[0]) / 100).round(0);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round(0);
                            item.additionaldiscount += (item.receivable * parseFloat(discountparts[1]) / 100).round(0);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round(2);
                            item.additionaldiscountpercentage = ((item.additionaldiscount / item.core.salerate) * 100).round(2);
                            $scope.ComputeTotal();
                        }

                    }

                    $timeout(function () {
                        $scope.setFocusdiscountpercentage();
                        $scope.ComputeTotal();
                        $scope.$digest();
                    }, 10);

                    $scope.RequestSMS = function () {
                        if ($scope.sale_requestpasscodetosms) {

                            var totaldiscount = 0;
                            var totalreceivable = 0;
                            $.each($scope.PopupList, function (k, v) {
                                totaldiscount += v.additionaldiscount;
                                totalreceivable += v.receivable;
                            });
                            totalreceivable = $rootScope.RoundRupee(totalreceivable, $scope.system_salesrounding);
                            var request = {
                                "Item": {
                                    'code': $scope.EmployeeCode,
                                    'locationid': $rootScope.LocationInfo.location.id,
                                    'permission': 'CANAPPROVEDISCOUNT',
                                    'source':
                                    {
                                        'receivable': totalreceivable, 'discount': totaldiscount
                                    }
                                }
                            }

                            $http({
                                method: 'POST',
                                url: "UsersService/RequestPASSCODE",
                                data: JSON.stringify(request)
                            }).success(function (result) {

                                if (typeof (result) == 'undefined' || !result.success) {
                                    $rootScope.Toaster.Notice('Given code not available');
                                    return;
                                }

                                if (!result.permission.view) {
                                    $rootScope.Toaster.Notice(result.employee + " don't have permission to approve");
                                    return;
                                }

                                $rootScope.Toaster.Notice(result.employee + " approval requested for discount");
                                $scope.AD = parseFloat(result.alloweddiscount);
                                if (isNaN($scope.AD)) $scope.AD = 0;
                                $scope.CanAskPasscode = true;
                                $timeout(function () {
                                    $scope.setFocuspasscode();
                                    $scope.$digest();
                                }, 10);

                            });
                        }
                    }

                    $scope.RequestPermission = function () {

                        $http({
                            method: 'GET',
                            url: "UsersService/Passcode/" + $scope.EmployeeCode + "/" + $scope.PassCode + "/CANAPPROVEDISCOUNT/" + $rootScope.LocationInfo.location.id,
                            data: null
                        }).success(function (result) {
                            if (typeof (result) == 'undefined' || !result.success) {
                                $rootScope.Toaster.Notice('Given code not available');
                                return;
                            }

                            if (!result.permission.view) {
                                $rootScope.Toaster.Notice(result.employee + " don't have permission to approve");
                                return;
                            }

                            $rootScope.Toaster.Notice(result.employee + " approved for discount");
                            $scope.AD = parseFloat(result.alloweddiscount);
                            if (isNaN($scope.AD)) $scope.AD = 0;
                            $scope.CanApprove = true;
                            $scope.Approve();
                        });
                    }

                    $scope.Approve = function () {

                        if ($scope.discountpercentage > $scope.AD) {
                            $rootScope.Toaster.Notice("Your allowed discount maximum @" + $scope.AD + '%');
                            return;
                        }
                        $scope.closeThisDialog({ 'items': $scope.PopupList, 'status': 'approved', 'rounding': $scope.RoundingValue });
                    };

                    $scope.Request = function () {
                        $scope.closeThisDialog({ 'items': $scope.PopupList, 'status': 'request', 'rounding': $scope.RoundingValue });
                    };

                    $scope.CompleteDiscount = function (option) {
                        if (option == '%' && $scope.discountpercentage.toString().indexOf('+') > 0) {

                            var discountparts = $scope.discountpercentage.toString().split('+');
                            var totaldiscount = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                v.additionaldiscount = (v.core.salerate * parseFloat(discountparts[0]) / 100).round(0);
                                v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                v.additionaldiscount += (v.receivable * parseFloat(discountparts[1]) / 100).round(0);
                                v.additionaldiscountpercentage = ((v.additionaldiscount / v.core.salerate) * 100).round(2);
                                v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                v.receivable = v.receivable.round(2);
                                totaldiscount += v.additionaldiscount;
                                $scope.discountpercentage = v.additionaldiscountpercentage;
                            });
                            $scope.discountvalue = totaldiscount;
                            $scope.ComputeTotal();
                            return;
                        } else if (option == '%' && $scope.discountpercentage.toString().toLowerCase() == 'r') {

                            var plist = $scope.GetSelectedList();
                            for (var index = 0; index < plist.length; index++) {

                                plist[index].additionaldiscount = (plist[index].core.discount * -1 / plist[index].core.qty).round(2);
                                plist[index].additionaldiscountpercentage = plist[index].core.discountpercentage * -1;

                                plist[index].receivable = plist[index].core.receivable - plist[index].core.discount;
                                plist[index].receivable = plist[index].receivable.round(2);
                            }

                        }

                        if ($scope.CanApprove) {
                            $scope.Approve();
                        }
                        else {
                            $scope.setFocusemployeecode();
                        }
                    }

                    $scope.DiscountChanged = function (type, item, newvalue, oldvalue) {
                        if (type == '%') {
                            if (newvalue.toString().indexOf('+') > 0)
                                return;
                            item.additionaldiscount = (item.core.salerate * newvalue / 100).round(2);
                            item.additionaldiscountpercentage = newvalue.round(2);

                            if ($scope.system_b2bexclusivetax) {
                                item.etdiscount = (item.core.etprice * newvalue / 100).round($scope.rounding_discount);
                                item.etdiscountpercentage = newvalue.round($scope.rounding_discount);
                            }

                        }
                        else {
                            if ($scope.system_b2bexclusivetax) {
                                item.etdiscount = newvalue.round($scope.rounding_discount);
                                item.etdiscountpercentage = ((item.etdiscount / item.core.etprice) * 100).round($scope.rounding_discount);
                            }
                            item.additionaldiscount = newvalue.round(2);
                            item.additionaldiscountpercentage = ((newvalue / item.core.salerate) * 100).round(2);
                        }
                        item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round(2);
                        $scope.ComputeTotal();
                    };

                    $scope.GetSelectedList = function () {
                        var lst = [];
                        $.each($scope.PopupList, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected)
                                lst.push(v);
                        });
                        if (lst.length == 0)
                            return $scope.PopupList;
                        return lst;
                    }

                    $scope.DirectDiscountChanage = function (type, newvalue, oldvalue) {

                        newvalue = parseFloat(newvalue);
                        if (isNaN(newvalue))
                            newvalue = 0;

                        $scope.TotalValue = 0;
                        $.each($scope.GetSelectedList(), function (k, v) {
                            $scope.TotalValue += v.core.receivable;
                        });

                        if (type == '%') {
                            var totaldiscount = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                if ($scope.IsSaleReturn || v.core.qty > 0) {
                                    if ($scope.system_b2bexclusivetax) {
                                        v.etdiscount = (v.core.etprice * newvalue / 100).round($scope.rounding_discount);
                                        v.etdiscountpercentage = ((v.etdiscount / v.core.etprice) * 100).round($scope.rounding_discount);
                                    }
                                    v.additionaldiscount = (v.core.salerate * newvalue / 100).round(2);
                                    v.additionaldiscountpercentage = newvalue;
                                    v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                    v.receivable = v.receivable.round(2);
                                }
                                totaldiscount += v.additionaldiscount;
                            });
                            $scope.discountvalue = totaldiscount;
                        }
                        else {
                            var avail = newvalue;
                            var percent = (avail / $scope.TotalValue * 100).round(2);
                            $scope.discountpercentage = percent;
                            var plist = $scope.GetSelectedList();
                            for (var index = 0; index < plist.length; index++) {
                                var item = plist[index];
                                if ($scope.IsSaleReturn || item.core.qty > 0) {
                                    if ($scope.system_b2bexclusivetax) {
                                        if (index == (plist.length - 1)) {
                                            item.etdiscount = (avail / Math.abs(item.core.qty)).round($scope.rounding_discount);
                                            item.etdiscountpercentage = ((item.etdiscount / (item.core.etprice)) * 100).round($scope.rounding_discount);
                                        }
                                        else {
                                            item.etdiscountpercentage = percent;
                                            item.etdiscount = (percent * item.core.etprice / 100).round($scope.rounding_discount);
                                            avail -= (item.etdiscount * Math.abs(item.core.qty));
                                        }
                                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                                        item.additionaldiscountpercentage = item.etdiscountpercentage;
                                        item.additionaldiscount = (baserate * item.etdiscountpercentage / 100).round($scope.rounding_discount);
                                        item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round($scope.rounding_discount);
                                    }
                                    else {
                                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                                        if (index == (plist.length - 1)) {
                                            item.additionaldiscount = (avail / Math.abs(item.core.qty)).round($scope.rounding_discount);
                                            item.additionaldiscountpercentage = ((item.additionaldiscount / (baserate)) * 100).round($scope.rounding_discount);
                                        }
                                        else {
                                            item.additionaldiscountpercentage = percent;
                                            item.additionaldiscount = ((baserate) * percent / 100).round($scope.rounding_discount);
                                            avail -= (item.additionaldiscount * Math.abs(item.core.qty));
                                        }
                                        item.receivable = (item.core.receivable + (item.core.additionaldiscount * item.core.qty)) - (item.additionaldiscount * item.core.qty);
                                        item.receivable = item.receivable.round($scope.rounding_amount);
                                    }
                                    if (!$scope.IsSaleReturn && item.receivable < 0) {
                                        item.additionaldiscount = item.core.additionaldiscount;
                                        item.additionaldiscountpercentage = item.core.additionaldiscountpercentage;
                                        item.receivable = item.core.receivable;
                                    }
                                }
                            }
                        }
                        $scope.ComputeTotal();
                    };

                    $scope.DiscountChanged = function (type, item, newvalue, oldvalue) {
                        if (type == '%' && newvalue.toString().indexOf('+') > 0) {
                            return;
                        }

                        newvalue = parseFloat(newvalue);
                        if (isNaN(newvalue))
                            newvalue = 0;

                        if (oldvalue == newvalue) return;
                        if (type == '%') {
                            item.additionaldiscount = (item.core.salerate * newvalue / 100).round(2);
                            item.additionaldiscountpercentage = newvalue.round(2);
                        }
                        else {
                            item.additionaldiscount = newvalue.round(2);
                            item.additionaldiscountpercentage = ((item.additionaldiscount / item.core.salerate) * 100).round(2);
                        }
                        item.receivable = ((item.core.receivable + (item.core.additionaldiscount * item.core.qty)) - (item.additionaldiscount * item.core.qty)).round(2);

                        if ($scope.TotalValue == -1) {
                            $scope.TotalValue = 0;
                            $.each($scope.PopupList, function (k, v) {
                                $scope.TotalValue += (v.core.receivable + v.core.additionaldiscount);
                            });
                        }
                        totaldiscount = 0;
                        $.each($scope.PopupList, function (k, v) {
                            totaldiscount += v.additionaldiscount;
                        });
                        $scope.discountpercentage = (totaldiscount / $scope.TotalValue * 100).round(2);
                        $scope.discountvalue = totaldiscount;
                        $scope.ComputeTotal();
                    };

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {

                        if ($scope.system_salesrounding > 0 && data.value.rounding != 0) {
                            var maxDiscount = data.value.items[0];
                            maxDiscount.additionaldiscount = $scope.GetNumber(maxDiscount.additionaldiscount);

                            $.each(data.value.items, function (k, v) {
                                if (maxDiscount.additionaldiscount < $scope.GetNumber(v.additionaldiscount)) {
                                    maxDiscount = v;
                                    maxDiscount.additionaldiscount = $scope.GetNumber(maxDiscount.additionaldiscount);
                                }
                            });
                            maxDiscount.additionaldiscount -= data.value.rounding;
                            maxDiscount.additionaldiscount.round(2);
                        }

                        $scope.CurrentBill.tax = 0;
                        $scope.CurrentBill.discount = 0;
                        $scope.CurrentBill.receivable = 0;
                        $scope.CurrentBill.rounding = 0;
                        $scope.CurrentBill.total = 0;
                        $scope.CurrentBill.additionaldiscount = 0;
                        $.each(data.value.items, function (k, v) {
                            v.core.additionaldiscount = $scope.GetNumber(v.additionaldiscount).round(2);
                            v.core.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage).round(2);
                            v.core.etdiscount = $scope.GetNumber(v.etdiscount);
                            v.core.etdiscountpercentage = $scope.GetNumber(v.etdiscountpercentage);

                            if (typeof (v.core.barcodeDetail) == 'undefined') {
                                v.core.barcodeDetail = { 'taxid': v.core.taxid, 'companyid': v.core.companyid };

                                if (v.core.taxid > 0) {
                                    v.core.taxInfo = $scope.SalesTax.find(function (arritem) {
                                        return arritem.id == v.core.taxid;
                                    });

                                    if (v.core.taxInfo != null)
                                        v.core.taxInfo.taxsplit.findTax = $scope.findTax;
                                }
                                v.core.barcodeDetail.discount = v.core.discount;
                                v.core.barcodeDetail.additionaldiscount = 0;
                            }
                            $scope.UpdateTax(v.core);
                            $scope.UpdateItemInfo(v.core, false);
                        });
                        $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                        $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                    }
                }
            });
        }

        //template: 'SettlementDialog',

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

                    $scope.CurrentBill.billattributes.settlement = data.value;

                    if ($scope.ValidateBill()) {
                        $scope.SaveBill();
                    }
                }
            });
        }

        $scope.SaveBill = function () {

            $scope.CurrentBill.isb2b = true;
            var request = {
                "Item": $scope.CurrentBill
            }
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "BillService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result.id > 0) {

                    var allsaved = true;
                    $.each(result.Bills, function (k, v) {
                        if (v.id <= 0)
                            allsaved = false;
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
                        if ($scope.DiscountQueue && !$scope.discounttimerenabled && $scope.CurrentBill.additionaldiscount > 0) {
                            $scope.discounttimerenabled = true;
                            $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                        }

                        $scope.AddBillHistory(result, false, true)
                        $scope.CreateNew();
                        if ($scope.DiscountQueue) {
                            $scope.AllowAdditionalDiscount = false;
                        }
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
                $scope.ErrorMessage = result;
                $scope.SaveRequest = false;
            });
        }

        $scope.AdditionalChargeItem_Add = function () {

            if ($scope.AdditionalChargeItem.Charge.toString().endsWith('%')) {
                $scope.AdditionalChargeItem.Charge = ($scope.CurrentBill.total * $scope.GetNumber($scope.AdditionalChargeItem.Charge.toString().replace('%', '')) / 100).round(0);
                sourcevalue = $scope.AdditionalChargeItem.Charge;
                $scope.AdditionalChargeItem_Changed(false);
                return;
            }

            $scope.CurrentBill.additionalchargeslist.push(JSON.parse(JSON.stringify($scope.AdditionalChargeItem)));
            $scope.ComputeAddtionalChargeValues();
            $scope.setFocusaddtaxvalue();
        }

        $scope.AdditionalChargeItem_Changed = function (fromtype) {
            var sourcevalue = $scope.GetNumber($scope.AdditionalChargeItem.Charge);
            $scope.AdditionalChargeItem.Tax = 0;
            $scope.AdditionalChargeItem.Amount = 0;
            $scope.AdditionalChargeItem.TaxId = 0;
            $scope.AdditionalChargeItem.TaxPercentage = 0;

            var item = $scope.AdditionalChargeList.find(function (item) {
                return $scope.AdditionalChargeItem.ChargeID == item.id
            });
            if (typeof (item) != 'undefined' && item != null) {

                if (fromtype && item.additionalinfo.billdiscount > 0) {
                    if (item.additionalinfo.billdiscountinclude) {
                        $scope.AdditionalChargeItem.Charge = ($scope.CurrentBill.total * item.additionalinfo.billdiscount / 100).round(0);
                        sourcevalue = $scope.AdditionalChargeItem.Charge;
                    }
                    else {
                        $scope.AdditionalChargeItem.Charge = (($scope.CurrentBill.total + $scope.CurrentBill.additionalcharges) * item.additionalinfo.billdiscount / 100).round(0);
                        sourcevalue = $scope.AdditionalChargeItem.Charge;
                    }
                }

                $scope.AdditionalChargeItem.additionalchargestypeid = item.id;
                $scope.AdditionalChargeItem.TaxId = item.referenceid;
                $scope.AdditionalChargeItem.Name = item.name;
                var taxitem = $scope.SalesTax.find(function (titem) {
                    return titem.id == item.referenceid;
                });
                if (typeof (taxitem) != 'undefined' && taxitem != null) {

                    if (taxitem.taxsplit == null) {
                        taxitem.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                    }
                    taxitem.taxsplit.findTax = $scope.findTax;
                    var taxitemvalue = taxitem.taxsplit.findTax(sourcevalue);
                    $scope.AdditionalChargeItem.TaxPercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                    $scope.AdditionalChargeItem.Tax = ($scope.AdditionalChargeItem.TaxPercentage * sourcevalue / 100).round(5);
                }
            }
            $scope.AdditionalChargeItem.Amount = (sourcevalue + $scope.AdditionalChargeItem.Tax).round(0);
        }

        $scope.ComputeAddtionalChargeValues = function () {
            $scope.CurrentBill.additionalcharges = 0;
            $.each($scope.CurrentBill.additionalchargeslist, function (k, v) {
                $scope.CurrentBill.additionalcharges += v.Amount;
            });
        }

        $scope.AdditionalChargeItem_Remove = function (source) {
            $scope.CurrentBill.additionalchargeslist.splice($scope.CurrentBill.additionalchargeslist.indexOf(source), 1);
            $scope.ComputeAddtionalChargeValues();
        }

        $scope.UpdateSalesOrder = function (list) {
            var dialog = ngDialog.open({
                template: 'SalesOrderDialog',
                data: [$scope, list],
                width: '800px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.PopupList = $scope.ngDialogData[1];
                    $scope.Import = function () {
                        $scope.closeThisDialog($scope.PopupList);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var soitemlist = data.value;
                        var list = [];
                        $.each(soitemlist, function (k, v) {
                            list.push({
                                'soitemid': v.soitemid,
                                'soid': v.soid,
                                'qty': v.qty,
                                'uid': v.uid
                            });
                        });

                        var srequest = JSON.stringify({
                            "Item": {
                                'locationid': $rootScope.LocationInfo.location.id,
                                'items': list
                            },
                            "securitytoken": "String content"
                        });

                        $http({
                            method: 'POST',
                            url: "SalablegoodsService/BatchListBySO",
                            data: srequest
                        }).success(function (result) {
                            var baseSalesDetail = (JSON.parse(JSON.stringify($scope.SalesDetail)));

                            for (var i = 0; i < result.length; i++) {
                                var results = result[i];
                                $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                                $scope.SalesDetail.barcode = results.barcode;
                                $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                                $scope.SalesDetail.hsncode = results.hsncode;
                                $scope.SalesDetail.attributes = {
                                    'serialno': results.serialno, 'so': [{ 'id': results.soitemid, 'qty': results.qty }]
                                };
                                $scope.SalesDetail.taxid = results.taxid;
                                $scope.SalesDetail.additionaldiscountpercentage = 0;
                                results.price = 0;
                                results.discount = 0;
                                results.discountpercentage = 0;
                                results.schemediscount = 0;
                                results.schemediscountpercentage = 0;
                                $scope.SalesDetail.discountpercentage = 0;
                                $scope.SalesDetail.schemediscountpercentage = 0;
                                $scope.SalesDetail.designid = results.designid;
                                $scope.SalesDetail.brand = results.brandname;
                                $scope.SalesDetail.size = results.sizename;

                                $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                                    return arritem.id == $scope.SalesDetail.taxid;
                                });

                                if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                                    $scope.SalesDetail.taxInfo.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                                    var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.baseprice);
                                    $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                                }

                                var soitem = soitemlist.find(function (item) {
                                    return item.soitemid == results.soitemid;
                                });

                                if ($scope.system_b2bexclusivetax) {
                                    $scope.SalesDetail.etprice = soitem.price;
                                    $scope.SalesDetail.salerate = $scope.SalesDetail.etprice;
                                    $scope.SalesDetail.etdiscount = 0;
                                    $scope.SalesDetail.additionaldiscount = 0;

                                    if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null) {
                                        $scope.SalesDetail.taxpercentage = $scope.SalesDetail.taxInfo.taxpercentage;
                                        $scope.SalesDetail.taxname = $scope.SalesDetail.taxInfo.name;
                                        $scope.SalesDetail.baseprice = ($scope.SalesDetail.etprice - $scope.SalesDetail.etprice);

                                        if (!$scope.CurrentBill.isinterstatesale) {
                                            var halfTax = ($scope.SalesDetail.etprice * ($scope.SalesDetail.taxpercentage / 2) / 100).round(5);
                                            $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                                        }
                                        else {
                                            $scope.SalesDetail.itemtax = ($scope.SalesDetail.etprice * $scope.SalesDetail.taxpercentage / 100).round(5);
                                        }

                                        $scope.SalesDetail.price = ($scope.SalesDetail.etprice + $scope.SalesDetail.itemtax).round(2);
                                        $scope.SalesDetail.salerate = ($scope.SalesDetail.etprice + $scope.SalesDetail.itemtax).round(5);
                                        $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(5);
                                        $scope.SalesDetail.amount = (($scope.SalesDetail.salerate * $scope.SalesDetail.qty)).round(2);
                                    }
                                }
                                else {
                                    $scope.SalesDetail.etprice = soitem.price;
                                    $scope.SalesDetail.salerate = $scope.SalesDetail.etprice;
                                    $scope.SalesDetail.price = $scope.SalesDetail.etprice;
                                    if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null) {
                                        $scope.SalesDetail.taxpercentage = $scope.SalesDetail.taxInfo.taxpercentage;
                                        $scope.SalesDetail.taxname = $scope.SalesDetail.taxInfo.name;
                                        if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                                            $scope.SalesDetail.additionaldiscount = 0;

                                        if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount))
                                            $scope.SalesDetail.schemediscount = 0;

                                        $scope.SalesDetail.baseprice = $scope.SalesDetail.etprice - $scope.GetNumber($scope.SalesDetail.etdiscount);

                                        if (!$scope.CurrentBill.isinterstatesale) {
                                            var halfTax = ($scope.SalesDetail.price * ($scope.SalesDetail.taxpercentage / 2) / 100).round(5);
                                            $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                                        }
                                        else {
                                            $scope.SalesDetail.itemtax = ($scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 100).round(5);
                                        }
                                        $scope.SalesDetail.etprice = $scope.SalesDetail.price - $scope.SalesDetail.itemtax;
                                        $scope.SalesDetail.salerate = ($scope.SalesDetail.price).round(2);
                                        $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(5);
                                        $scope.SalesDetail.amount = (($scope.SalesDetail.salerate * $scope.SalesDetail.qty)).round(2);
                                    }
                                }

                                $scope.SalesDetail.qty = results.qty;
                                $scope.SalesDetail.barcode = results.barcode;
                                $scope.SalesDetail.imageid = results.imageid;
                                $scope.SalesDetail.productid = results.productid;
                                $scope.SalesDetail.sellingmode = results.sellingmode;
                                $scope.SalesDetail.printingname = results.sellingname;

                                $scope.SalesDetail.locationid = $rootScope.LocationInfo.location.id;
                                $scope.SalesDetail.companyid = results.companyid;
                                $scope.SalesDetail.receivable = $scope.SalesDetail.baseprice * $scope.SalesDetail.qty;
                                $scope.SalesDetail.cost = results.buyingprice;
                                $scope.SalesDetail.margin = (($scope.SalesDetail.baseprice / results.buyingprice * 100) - 100).round(2);
                                $scope.UpdateTax($scope.SalesDetail);
                                $scope.SalesDetail.barcodeDetail = results;
                                $scope.AddBillItem(true);
                            }
                        });

                    }
                }
            });
        }


        var PriceListManager = function (categorypricelist) {
            var PriceListCollection = [];
            var PriceList = function (customerid, customercategoryid, result) {
                var List = [];
                this.CustomerID = customerid;
                this.CategoryID = customercategoryid;
                this.GetPrice = function (design) {
                    var price = List.find(function (item) {
                        return item.id == design;
                    });
                    if (price && price != null)
                        return price.price;
                    return 0;
                }
                this.AddPrice = function (priceitem) {
                    List.push(priceitem);
                }
                $.each(result, function (k, v) {
                    List.push({ 'id': v.design, 'price': v.price });
                });
            };
            this.LoadPrice = function (customerid) {
                var pricelist = PriceListCollection.find(function (item) {
                    return item.CustomerID == customerid;
                })
                if (typeof (pricelist) == 'undefined' || pricelist == null) {
                    restService.Get('CustomerpriceService/SearchAll/' + customerid, true).then(function (result) {
                        var pricelist = new PriceList(customerid, 0, result);
                        PriceListCollection.push(pricelist);
                    });
                }
            }
            this.GetPrice = function (customerid, categoryid, design, callback) {
                var pricelist = PriceListCollection.find(function (item) {
                    return item.CustomerID == customerid;
                })
                if (pricelist && pricelist != null) {
                    var price = pricelist.GetPrice(design);
                    callback(price);
                }
                else {
                    pricelist = PriceListCollection.find(function (item) {
                        return item.CategoryID == categoryid;
                    })
                    if (pricelist && pricelist != null) {
                        var price = pricelist.GetPrice(design);
                        callback(price);
                    }
                    else {
                        callback(0);
                    }
                }
            }
            $.each(categorypricelist, function (k, v) {
                var pricelist = PriceListCollection.find(function (item) {
                    return item.CategoryID == v.categoryid;
                });
                if (typeof (pricelist) == 'undefined' || pricelist == null) {
                    pricelist = new PriceList(0, v.categoryid, []);
                    PriceListCollection.push(pricelist);
                }
                pricelist.AddPrice({ 'id': v.design, 'price': v.price });
            });
        }
        function SaleOrderManager() {
            this.SaleOrderItems = null;
            this.DesignwiseQty = [];
            this.Enabled = false;
            this.Config = null;
            this.Parent = null;
            this.CustomerID = null;
            this.Initialize = function (parantScope) {
                this.SaleOrderItems = null;
                this.ParentScope = parantScope;
                if (typeof ($scope.system_saleorder) == 'undefined' || $scope.system_saleorder == null) {
                    this.Enabled = false;
                }
                else if ($scope.system_saleorder.design && $scope.system_saleorder.design != null) {
                    this.Enabled = true;
                    this.Config = $scope.system_saleorder;
                }
            }
            this.ValidateSO = function (customerid) {
                return !this.Enabled || customerid == this.CustomerID;
            }

            this.qEncodeURIComponent = function (value) {
                if (value.indexOf('.') >= 0)
                    value = value.replaceAll('.', '~dot~');
                if (value.indexOf('/') >= 0)
                    value = value.replaceAll('/', '~sls~');
                return encodeURIComponent(value);
            }

            this.SearchSaleOrder = function (order, billitems, callback) {
                this.CustomerID = 0;
                this.SaleOrderItems = null;
                if (typeof (order) != 'undefined' && order != null && order.length > 0) {
                    var service = this;
                    this.DesignwiseQty = [];
                    var DesignwiseQty = this.DesignwiseQty;
                    var currentbillitems = null;
                    if (typeof (callback) != 'undefined') {
                        currentbillitems = billitems;
                    }
                    restService.Get('SalesorderService/GetSaleOrder/' + $rootScope.LocationInfo.location.id + '/' + this.qEncodeURIComponent(order)+ '/' + ($scope.IsSaleReturn ? 2 : 0), true).then(function (results) {
                        if (results.length == 0) {
                            $rootScope.ShowAlert('Sales Order', "Can't find the given sales order - " + order);
                        }
                        else {
                            service.SaleOrderItems = results;
                            service.CustomerID = results[0].customerid;
                            if (currentbillitems == null &&
                                results[0].customerid != service.ParentScope.CurrentBill.customerid) {
                                service.ParentScope.CurrentBill.customerid = results[0].customerid;
                                if (typeof ($scope.CurrentBill.billattributes) == 'undefined') $scope.CurrentBill.billattributes = {};
                                if (results[0].shipto > 0) {
                                    $scope.CurrentBill.billattributes.hasshiptto = true;
                                    $scope.CurrentBill.billattributes.shiptoid = results[0].shipto;
                                }
                                service.ParentScope.CustomerChanged(true);
                            }

                            if (service.ParentScope.CurrentBill.id && service.ParentScope.CurrentBill.id > 0) {
                                $scope.CurrentBill.billattributes.hasshiptto = false;
                                //currentbillitems = $scope.CurrentBill.Billitems;
                                if ($scope.CurrentBill.billattributes.shiptoid &&
                                    $scope.CurrentBill.billattributes.shiptoid > 0) {
                                    $scope.CurrentBill.billattributes.hasshiptto = true;
                                }
                            }

                            if (currentbillitems && currentbillitems != null) {
                                $.each(currentbillitems, function (bk, bv) {
                                    if (bv.attributes.so && (typeof (bv.attributes.soqty) == 'undefined' || bv.attributes.soqty == 0)) {
                                        bv.attributes.soqty = 0;
                                        $.each(bv.attributes.so, function (sk, sv) {
                                            bv.attributes.soqty += sv.qty;
                                        })
                                    }
                                });
                            }

                            $.each(results, function (k, v) {
                                v.so = [];
                                v.allocated = 0;
                                var dqty = DesignwiseQty.find(function (item) {
                                    return v.designitemcode == item.code;
                                });
                                if (typeof (dqty) == 'undefined' || dqty == null) {
                                    DesignwiseQty.push({ 'qty': v.pendingqty, 'code': v.designitemcode });
                                }
                                else {
                                    dqty.qty += v.pendingqty;
                                }
                            });
                            if (typeof (callback) != 'undefined')
                                callback();
                            else
                                service.ParentScope.UpdateSalesOrder(service.SaleOrderItems);
                        }
                    });
                }
            }
            this.Clear = function () {
                if (this.SaleOrderItems != null) {
                    $.each(this.SaleOrderItems, function (k, v) {
                        v.so = [];
                        v.allocated = 0;
                    });
                }
                $.each(this.CurrentBill.billitems, function (k, v) {
                    if (typeof (v.attributes) == 'undefined') v.attributes = {};
                    v.attributes.so = [];
                })
            }

            this.RemoveItem = function (billitem) {
                if (!this.Enabled || this.SaleOrderItems == null) return;
                var salesorderitems = this.SaleOrderItems;
                $.each(billitem.attributes.so, function (k, v) {
                    var soitem = salesorderitems.find(function (item) {
                        return item.soitemid == v.id;
                    });
                    if (soitem && soitem != null) {
                        soitem.allocated -= soitem.qty;
                    }
                })
            }

            this.UpdateOrder = function (groupitems) {
                if (this.Enabled && this.SaleOrderItems != null) {
                    var designqty = this.DesignwiseQty;
                    $.each(groupitems, function (k, v) {
                        var gqty = designqty.find(function (item) {
                            return item.code == v.designitemcode || item.code == v.printingname;
                        })
                        if (gqty && gqty != null) {
                            v.attributes.somissing = false;
                            v.soqty = gqty.qty;
                            var somissing = v.list.find(function (item) {
                                return item.attributes.somissing
                            });
                            if (somissing && somissing != null) {
                                v.attributes.somissing = true;
                                v.attributes.somismatch = true;
                            }
                            else {
                                if (v.qty != v.soqty) {
                                    v.attributes.somismatch = true;
                                    v.attributes.somissing = false;
                                }
                                if (v.qty > v.soqty) {
                                    v.attributes.somissing = true;
                                    v.attributes.somismatch = false;
                                }
                            }
                        }
                        else {
                            v.attributes.somissing = true;
                            v.soqty = 0;
                        }
                    });
                }
            }


            this.UpdateItem = function (billitem, callback) {
                if (typeof (billitem.attributes) == 'undefined') billitem.attributes = {};
                billitem.attributes.so = [];
                if (this.Enabled) {
                    if (this.SaleOrderItems != null) {
                        var billed = billitem.qty;
                        $.each(billitem.attributes.so, function (k, v) {
                            var soitem = this.SaleOrderItems.find(function (item) {
                                return item.soitemid == v.id;
                            });
                            if (soitem && soitem != null) {
                                if (isNaN(soitem.allocated)) soitem.allocated = 0;
                                soitem.allocated -= soitem.qty;
                            }
                        })
                        billitem.attributes.so = [];
                        var required = 0;
                        var ordered = 0;
                        var allocated = 0;
                        for (var i = 0; i < this.SaleOrderItems.length; i++) {
                            var soitem = this.SaleOrderItems[i];
                            if (soitem.productid == billitem.productid &&
                                soitem.uid == billitem.attributes.serialno) {
                                if (isNaN(soitem.allocated)) soitem.allocated = 0;
                                ordered += soitem.pendingqty;
                                allocated += soitem.allocated;
                            }
                        }
                        if (ordered <= 0) {
                            billitem.attributes.somissing = true;
                            this.ParentScope.Toaster.Info('SO Not available for this UAN - ' + billitem.attributes.serialno);
                        }
                        else {
                            for (var i = 0; i < this.SaleOrderItems.length && billed > 0; i++) {
                                var soitem = this.SaleOrderItems[i];
                                if (soitem.productid == billitem.productid &&
                                    soitem.uid == billitem.attributes.serialno) {
                                    if (isNaN(soitem.allocated)) soitem.allocated = 0;
                                    available = soitem.pendingqty - soitem.allocated;
                                    if (available >= billed) {
                                        billitem.attributes.so.push({
                                            'id': soitem.soitemid,
                                            'qty': billed
                                        });
                                        allocated += billed;
                                        billed = 0
                                    }
                                    else {
                                        billitem.attributes.so.push({ 'id': soitem.soitemid, 'qty': billed });
                                        billed -= available;
                                        allocated += available;
                                    }
                                }
                            }
                            billitem.attributes.soqty = billitem.qty - billed;
                            if (allocated != ordered || allocated != billitem.qty) {
                                billitem.attributes.somissing = true;
                                this.ParentScope.Toaster.Info('Pending - ' + (ordered - allocated) + '(' + ordered + '), Billed -' + billitem.qty);
                            }
                            else {
                                billitem.attributes.somissing = false;
                            }
                        }
                    }
                    else {
                        billitem.attributes.somissing = true;
                    }
                }
                if (typeof (callback) != 'undefined') {
                    callback();
                }
            }
        }

        $scope.SOManager = new SaleOrderManager();
        $scope.priceList = new PriceListManager([]);

        $scope.ShowDirectSettlement = function (bill) {
            var dialog = ngDialog.open({
                template: 'clientside/views/Settlements_dialog.html',
                data: [$scope, bill],
                width: '1000px',
                controller: 'SettlementsController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.setFocusautocoding();
                }
            });
        }

        $scope.GridConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.options.rowHeight = 30;
            igrid.AddSerialNo(40);

            igrid.AddSimpleColumn('barcode', 'Barcode', 60);
            var column = igrid.AddCustomColumn('brand', 'Brand', 100, function (item, col) {
                return item.attributes.brand;
            });
            var column = igrid.AddCustomColumn('designid', 'Design', 100, function (item, col) {
                return item.attributes.designid;
            });
            if (!$scope.system_hidecost) {
                var column = igrid.AddCustomColumn('cost', 'Cost', 60, function (item, col) {
                    return item.attributes.cost;
                });
                column.cssClass = 'rightalign';
            }

            if (!$scope.system_supportpriceedit) {
                var column = igrid.AddCustomColumn('margin', 'Margin', 60, function (item, col) {
                    return item.attributes.margin;
                });
                column.cssClass = 'rightalign';
            }

            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2);

            var column = igrid.AddCustomColumn('gross', 'Taxable', 80, function (item, col) {
                return $filter('IndianNumber')(item.attributes.gross, 2);
            });
            column.cssClass = 'rightalign';

            igrid.AddFormatedColumn('discount', 'Discount', 60, 'N2', 2);

            var column = igrid.AddCustomColumn('adddiscount', 'A.D', 60, function (item, col) {
                var val = (item.qty * item.additionaldiscount) + item.schemediscount;
                if (val != 0) return $filter('IndianNumber')(val, 2);
                return '';
            });
            column.cssClass = 'rightalign';

            var column = igrid.AddCustomColumn('discountpercentage', '%', 60, function (item, col) {
                var val = item.discountpercentage + item.schemediscountpercentage + item.additionaldiscountpercentage;
                if (val != 0) return $filter('IndianNumber')(val, 2);
                return '';
            });
            column.cssClass = 'rightalign';

            igrid.AddFormatedColumn('salerate', 'Rate', 60, 'N2', 2);
            var column = igrid.AddCustomColumn('tax', 'Tax', 80, function (item, col) {
                var val = $filter('IndianNumber')(item.itemtax, 2);
                return val + ' ' + item.taxpercentage + '%';
            });
            column.cssClass = 'rightalign';
            igrid.AddFormatedColumn('receivable', 'Amount', 80, 'N2', 2);

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('SMChange', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SMChange' class='btn btn-info btn-xs'><i class='fa fa-user' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            //var commands = "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
            //commands += "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
            //commands += "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SMChange' class='btn btn-info btn-xs'><i class='fa fa-user' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
            //igrid.AddButtonColumn('action', 'ACTION', 110, commands, function (e, args) {
            //    $scope.$apply(function () {
            //        $scope.ManageBillItem($(e.target).val(), args.item);
            //    });
            //});

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    $scope.ManageBillItem(command, item);
                });
            });
            igrid.setCellCSS('igridbigrows');
            $scope.igrid = igrid;
            $scope.igrid.UpdateData([]);
        }

        $timeout(function () {
            $rootScope.slideToggeler.Toggle(true);

            $scope.$on(
                "$destroy",
                function (event) {
                    if ($scope.UpdateOffertimeout != null)
                        $timeout.cancel($scope.UpdateOffertimeout);
                }
            );
            $scope.GridConfig();

        }, 10);


        $scope.suggest_selected = null;

        function suggest_state(term) {
            $scope.suggest_selected = null;
            var q = term.toLowerCase().trim();
            var results = [];

            if (q.length < 3 || typeof ($scope.BillKeys) == 'undefined' || $scope.BillKeys == null)
                return results;

            for (var i = 0; i < $scope.BillKeys.length && results.length < 10; i++) {
                var state = $scope.BillKeys[i];
                if (state.name.toLowerCase().indexOf(q) === 0)
                    results.push({ label: $sce.trustAsHtml(state.name), value: state.name, obj: state });
            }
            return results;
        }

        $scope.HSNLimit = {
            max: 0,
            init: function (length) {
                this.max = length
            },
            validate: function (items, newitem) {
                if (this.max <= 0) return true;
                var hsnlist = [newitem.hsncode];
                $.each(items, function (k, v) {
                    if (hsnlist.indexOf(k.hsncode) < 0)
                        hsnlist.push(k.hsncode);
                });
                return hsnlist.length <= this.max;
            }
        }

        $scope.autocomplete_options = {
            suggest: suggest_state,
            on_select: function (selected) {
                $scope.suggest_selected = selected.obj;
            }
        };
        $scope.openNav = function () {
            document.getElementById("mySidenav").style.width = "50px";
        }
        $scope.closeNav = function () {
            document.getElementById("mySidenav").style.width = "0";
        }
    }]);