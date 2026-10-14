var app = angular.module('InfinityLab');
app.controller('SettlementsController', ['UserDetails', '$scope', '$http', '$location',
    '$rootScope', '$window', 'restService', '$filter', '$timeout', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout, ngDialog, localStorageService) {

        $scope.AllowMultiCompany = true;
        $scope.sale_canclaimsamebillrewards = false;
        $scope.system_loansettlement = false;
        $scope.system_swippingmachine = false;
        $scope.system_salecollectcarddata = false;
        $scope.system_salecollectagent = false;
        $scope.system_salecollectrefemp = false;
        $scope.system_supportcustcreditnote = false;
        $scope.settlement_disableadjustment = false;
        $scope.settlement_disablereturnbill = false;
        $scope.sale_requestpasscodetosms = false;
        $scope.system_posdisablerefund = false;
        $scope.int_finance = false;
        $scope.PaymentIntegration = null;
        $scope.IsAutoSettlement = typeof ($scope.ngDialogData) != 'undefined' && $scope.ngDialogData.length > 0;
        //$scope.SourceBill = typeof ($scope.ngDialogData) != 'undefined' && $scope.ngDialogData.length > 1 ? $scope.ngDialogData[1] : null; 
        $scope.SearchRef = restService.SearchReference;
        $scope.locationid = null;
        $scope.counterid = null;
        //$scope.billprefix = null;
        $scope.IsDaylight = false;
        $scope.dateStyle = '';
        $scope.settlement_maxadjustment = 5;
        $scope.lastMasterBillID = 0;
        $scope.lastrefundamount = 0;
        $scope.lastbillmastercode = '';
        $scope.lastbillno = '';
        $scope.unsettlebillcount = 0;
        $scope.BillNoConfig = {
            'multiple': false, 'prefix': '', 'bulkbillprefix': '', 'bulkbillprefixlist': [], 'prefixlist': [],
            'isbillno': function (key) {
                if (key.startsWith('#'))
                    return true;

                if (!this.multiple)
                    return (key.startsWith(this.offlineprefix + this.prefix));

                for (var i = 0; i < this.prefixlist.length; i++) {
                    if (key.startsWith(this.offlineprefix + this.prefixlist[i]))
                        return true;
                }

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
                    return (key.startsWith(this.offlineprefix + this.bulkbillprefix));

                for (var i = 0; i < this.bulkbillprefixlist.length; i++) {
                    if (key.startsWith(this.offlineprefix + this.bulkbillprefixlist[i]))
                        return true;
                }
                return false;
            },
            'issettlement': function (key) {
                if (key.startsWith('$'))
                    return true;

                if (!this.multiple)
                    return (key.startsWith(this.offlineprefix + this.billmasterprefix));

                for (var i = 0; i < this.billmasterprefixlist.length; i++) {
                    if (key.startsWith(this.offlineprefix + this.billmasterprefixlist[i]))
                        return true;
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

                    if (typeof ($rootScope.LocationInfo.billmasterconfig) != 'undefined' && $rootScope.LocationInfo.billmasterconfig != null) {
                        this.billmasterprefix = $rootScope.LocationInfo.billmasterconfig.prefix;
                        this.billmasterprefixlist = $rootScope.LocationInfo.billmasterconfig.prefixlist;
                    }

                    if (typeof (this.prefix) == 'undefined' || this.prefix == '')
                        this.prefix = null;

                    if (typeof (this.bulkbillprefix) == 'undefined' || this.bulkbillprefix == '')
                        this.bulkbillprefix = null;

                    if (typeof (this.billmasterconfig) == 'undefined' || this.billmasterconfig == '')
                        this.billmasterprefix = null;
                }
            }
        };

        $scope.autoCoding = { 'code': '' };
        $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.location) != 'undefined')
        $scope.HasCounterSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.counter) != 'undefined')
        $scope.OnKeyPress = function (value) {
            return value;
        };


        $scope.LoadPaymentIntegration = function () {

            if ($scope.SwippingList == null) {
                $timeout(function () {
                    $scope.LoadPaymentIntegration();
                }, 1000);
                return;
            }
            $scope.PaymentIntegration = new PaymentIntegration($rootScope.UserInfo.appsettings.int_paymode, $rootScope.UserInfo.appsettings,
                localStorageService, $http, $timeout, $scope, $rootScope);
            $scope.PaymentIntegration.CallBack = $scope.PaymentIntegrationCallBack;

            if ($scope.IsAutoSettlement) {
                $scope.PaymentIntegration.EnableSingleWindowNo($rootScope.LocationInfo.location.id,
                    $rootScope.LocationInfo.counter.id);
                if ($scope.PaymentIntegration != null &&
                    $scope.PaymentIntegration.Integrater &&
                    $scope.PaymentIntegration.Integrater != null) {
                    var swippingintegeration = localStorageService.get('swippingintegeration');
                    if (swippingintegeration && swippingintegeration != null) {
                        $scope.PaymentIntegration.optionid = swippingintegeration.optionid;
                        $scope.PaymentIntegration.deviceid = swippingintegeration.deviceid;
                        $timeout(function () {
                            $scope.PaymentIntegration.configChanged($scope.PaymentIntegration.deviceid, $scope.SwippingList);
                        }, 50);
                    }
                }
            }
        }

        $scope.LoadApplicationSettings = function () {
            if (!$scope.HasLocationSelected) {
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined');
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.location) != 'undefined')
                    $scope.HasCounterSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.counter) != 'undefined')
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
                if (result.length > 0) {
                    $scope.ApplicationSettings = result[0];

                    $scope.sale_canclaimsamebillrewards = false || $scope.ApplicationSettings.appsettings.sale_canclaimsamebillrewards;
                    $scope.AllowMultiCompany = !$scope.ApplicationSettings.appsettings.settlement_disallowmulticompany;
                    $scope.system_loansettlement = $scope.ApplicationSettings.appsettings.system_loansettlement;
                    $scope.system_swippingmachine = $scope.ApplicationSettings.appsettings.system_swippingmachine;
                    $scope.system_salecollectcarddata = $scope.ApplicationSettings.appsettings.system_salecollectcarddata;
                    $scope.system_systemrole = $scope.ApplicationSettings.appsettings.system_systemrole;
                    $scope.system_salecollectagent = false || $scope.ApplicationSettings.appsettings.system_salecollectagent;
                    $scope.system_salecollectrefemp = false || $scope.ApplicationSettings.appsettings.system_salecollectrefemp;
                    $scope.system_supportcustcreditnote = false || $scope.ApplicationSettings.appsettings.system_supportcustcreditnote;
                    $scope.settlement_disableadjustment = false || $scope.ApplicationSettings.appsettings.settlement_disableadjustment;
                    $scope.settlement_disablereturnbill = false || $scope.ApplicationSettings.appsettings.settlement_disablereturnbill;
                    $scope.sale_requestpasscodetosms = false || $scope.ApplicationSettings.appsettings.sale_requestpasscodetosms;
                    $scope.int_finance = false || $scope.ApplicationSettings.appsettings.int_finance;
                    $scope.system_salesroundingdigit = $scope.ApplicationSettings.appsettings.system_salesrounding == 99 ? 2 : 0;
                    $scope.settlement_maxadjustment = $scope.ApplicationSettings.appsettings.settlement_maxadjustment <= 0 ? 5 : $scope.ApplicationSettings.appsettings.settlement_maxadjustment;

                    $scope.system_posdisablerefund = $scope.ApplicationSettings.appsettings.system_posdisablerefund;
                    if ($scope.system_posdisablerefund &&
                        $rootScope.UserInfo &&
                        $rootScope.UserInfo.p_SUPPORTPOSREFUND &&
                        $rootScope.UserInfo.p_SUPPORTPOSREFUND.view) {
                        $scope.system_posdisablerefund = false;
                    }

                    if ($scope.ApplicationSettings.appsettings.system_datechange &&
                        $scope.ApplicationSettings.nextdaycount > 0 &&
                        $filter('date')($scope.ApplicationSettings.currentday, 'dd-MM-yyyy') == $filter('date')(new Date(), 'dd-MM-yyyy')) {
                        $scope.IsDaylight = true;
                        $scope.DaylightCount = $scope.ApplicationSettings.nextdaycount;
                        $scope.dateStyle = '#f7dcdc';
                        $scope.Settlement.settlementon = $scope.GetBillDate();
                    }

                    if ($rootScope.UserInfo.appsettings.int_payment) {
                        $scope.LoadPaymentIntegration();
                    }

                    $scope.BillNoConfig.init($scope.ApplicationSettings.appsettings.system_multicompany);
                    if ($scope.ApplicationSettings.appsettings.system_alwayscaps)
                        $scope.OnKeyPress = function (value) {
                            return value.toUpperCase();
                        };

                    if (!$scope.IsAutoSettlement)
                        $scope.ValidateCounter();

                    if ($scope.system_systemrole) {
                        var authData = localStorageService.get('authorizationData');
                        if (authData.systemrole != 'ALL' &&
                            authData.systemrole != 'SETTLEMENT' &&
                            authData.systemrole != 'BILLSETTLE') {
                            $rootScope.ShowAlert('System Violation', 'This system not alloted for settlement');
                            $timeout(function () {
                                $(location).attr('href', 'index.html');
                            }, 1000);
                            return;
                        }
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

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
            $scope.floorname = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.floorid, results) : 'Select Floor';
        });

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            $scope.ShowLocationInfo();
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
            $scope.ShowLocationInfo();
        });

        $scope.ShowLocationInfo = function () {

            if ($scope.HasCounterSelected) {
                $scope.counterid = $rootScope.LocationInfo.counter.id;
                $scope.CounterName = $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters);
            }
            else {
                $scope.CounterName = 'SELECT COUNTER';
            }

            if ($scope.HasLocationSelected) {
                $scope.locationid = $rootScope.LocationInfo.location.id;
                $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
            }
            else {
                $scope.LocationName = 'SELECT LOCATION';
            }

            if (!$scope.IsAutoSettlement &&
                $scope.HasCounterSelected &&
                $scope.HasLocationSelected) {
                $scope.ValidateCounter();
            }
        }

        $scope.LoanProviderList = restService.Get('ReferencelistService/LOANPROVIDER', true).then(function (results) {
            $scope.LoanProviderList = results;
        });

        $scope.UPIProviderList = restService.Get('ReferencelistService/UPIPROVIDER', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-' });
            }
            else {
                var printerData = localStorageService.get('LocationData');
                if (printerData != null) {
                    var locationid = printerData.location.id;
                    var newlist = [];
                    $.each(results, function (k, v) {
                        if (v.locationid == 0 || v.locationid == locationid)
                            newlist.push(v);
                    });
                    results = newlist;
                }
            }
            $scope.UPIProviderList = results;
        });

        $scope.SwippingList = null;
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

        $scope.CardTypeList = restService.Get('ReferencelistService/CARDTYPES', true).then(function (results) {
            $scope.CardTypeList = results;
        });

        $scope.DiscountRemaks = restService.Get('ReferencelistService/DISCOUNTREMARK', true).then(function (results) {
            $scope.DiscountRemaks = results;
        });

        $scope.EmployeeList = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-', 'code': '-' });
            }
            $scope.EmployeeList = results;
        });

        $scope.AgentList = restService.Get('ReferencelistService/AGENT/4', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-', 'code': '-' });
            }
            $scope.AgentList = results;
        });

        $scope.BankList = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-' });
            }
            else {
                var printerData = localStorageService.get('LocationData');
                if (printerData != null) {
                    var locationid = printerData.location.id;
                    var newlist = [];
                    $.each(results, function (k, v) {
                        if (v.locationid == 0 || v.locationid == locationid)
                            newlist.push(v);
                    });
                    results = newlist;
                }
            }
            $scope.BankList = results;
        });

        $scope.vouchercode = "";
        $scope.voucherlist = [];
        $scope.voucheramount = 0.0;
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

        $scope.CustomerTemplate = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "supportcredit": false,
            "creditlimit": 0.0,
            "creditrightsgivenby": 0,
            "mobileno": null,
            "customertype": 0,
            "name": null,
            "dateofbirth": "",
            "marriagedate": "",
            "loyelty": 0.0,
            "gender": null,
            "area": null,
            "married": false,
            "malekids": null,
            "femalekids": null,
            "loyeltyno": null,
            "code": null
        };

        $scope.LoayaltyCutomer = (JSON.parse(JSON.stringify($scope.CustomerTemplate)));

        $scope.CreateNew = function () {
            $scope.Settlement = (JSON.parse(JSON.stringify($scope.TemplateSettlement)));
            $scope.voucherlist = [];
            $scope.vouchercode = "";
            $scope.voucheramount = 0.0;
            $scope.Settlement.settlementon = $scope.GetBillDate();
            $scope.Settlement.datedisplay = $filter('date')($scope.Settlement.settlementon, 'dd-MM-yyyy');
            $scope.Settlement.datetimedisplay = $filter('date')($scope.Settlement.settlementon, 'hh:mm a');
            $scope.autoCoding.code = '';
        };



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
                $rootScope.ShowAlert('Please select the location', 'No-Location');
                return false;
            }
            return true;
        }

        $scope.PrintLastBillMaster = function () {
            $scope.PrintSettlement(0, '', $scope.lastbillmastercode);
        }

        $scope.PrintLastBill = function () {
            $scope.PrintSettlement(0, $scope.Settlement.lastBillID);
        }

        $scope.Print = function () {
            if (typeof ($scope.Settlement.id) != 'undefined' && $scope.Settlement.id > 0)
                $scope.PrintSettlement($scope.Settlement.id, '');
        }

        $scope.SearchCompany = function () {
            if ($scope.SalesDetail.salesmannameLast != null &&
                $scope.SalesDetail.salesmannameLast != "" &&
                $scope.SalesDetail.salesmannameLast == $scope.SalesDetail.salesmanname) {
                $scope.AddBillItem();
                $scope.setFocusBarcode();
                return;
            }
            restService.Get('EmployeeService/Code/' + $scope.UserInfo.OrganizationID + '/' + $scope.SalesDetail.salesmanname, true).then(function (results) {
                $scope.SalesDetail.salesmanname = results.name;
                $scope.SalesDetail.salesmannameLast = results.name;
                $scope.SalesDetail.salesmanid = results.id;
            });
        }

        $scope.NewVoucher = function () {
            $scope.vouchercode = $scope.Settlement.paymentinfo.voucher_last;
            if (typeof ($scope.vouchercode) != 'undefined' && $scope.vouchercode.toString().length > 0) {
                $scope.SearchVoucher(function () {
                    $scope.Settlement.paymentinfo.voucher_last = "";
                    if ($scope.Settlement.paymentinfo.voucher_paid > 0)
                        $scope.setFocusvoucher_reference();
                    else if ($scope.IsAutoSettlement)
                        $scope.setFocuscashpaid();
                    else
                        $scope.setFocusautocoding();
                });
            }
            else {
                if ($scope.IsAutoSettlement)
                    $scope.setFocuscashpaid();
                else
                    $scope.setFocusautocoding();
            }
        };

        $scope.SearchVoucher = function (callback) {
            if ($scope.vouchercode == null || $scope.vouchercode == "") {
                return;
            }
            request = {
                "Item": { 'code': $scope.vouchercode },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "VouchersService/SearchVouchers",
                data: srequest
            }).success(function (result) {
                //if (typeof ($scope.Settlement.paymentinfo.voucher_paid) == 'undefined' || $scope.Settlement.paymentinfo.voucher_paid==null)
                $scope.Settlement.paymentinfo.voucher_paid = 0;

                if (result == 'undefined' || result == null || result.length == 0)
                    $scope.Toaster.Error("Voucher not found or expired.");

                $.each(result, function (k, v) {

                    var exist = false;
                    $.each($scope.voucherlist, function (kk, vv) {
                        if (vv.code == $scope.vouchercode)
                            exist = true;
                    });

                    if (!exist) {
                        v.available = v.amount;
                        $scope.voucherlist.push(v);
                    }

                });
                $scope.PaidChanged();
                var balance = $scope.Settlement.balanceamount;

                $.each($scope.voucherlist, function (k, v) {
                    if (balance > v.available) {
                        $scope.Settlement.paymentinfo.voucher_paid += v.available;
                        v.amount = v.available;
                        balance -= v.available;
                    }
                    else {
                        $scope.Settlement.paymentinfo.voucher_paid += balance;
                        v.amount = balance;
                        balance = 0;
                    }
                });

                $scope.Settlement.paymentinfo.voucher_last = $scope.vouchercode;
                $scope.PaidChanged();

                if ($scope.Settlement.balanceamount <= 0) {
                    $scope.setFocusvoucher_reference();
                }

                if (typeof (callback) != 'undefined' && callback != null)
                    callback(true);

            }).error(function (result) {
            });
        }



        $scope.PrintSettlement = function (id, billno, billmasterno) {

            request = {
                "Item": { 'id': 0, 'billno': billno, 'billmasterno': billmasterno },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);


            $http({
                method: 'POST',
                url: "BillsettlementmasterService/Print",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0 && result.printdocument != null) {
                    $scope.Sent2Printer(result);
                }


            }).error(function (result) {
                $scope.ErrorMessage = result;
            });
        }



        $scope.SaveRequest = false;
        $scope.SearchBill = function (callback) {
            if (!$scope.CheckLocation())
                return;

            if (typeof ($scope.Settlement.billno) == 'undefined' || $scope.Settlement.billno == null || $scope.Settlement.billno == '')
                return;

            if (!$scope.HasLocationSelected) {
                $rootScope.Toaster.Error('Please select location');
                return;
            }

            request = {
                "Item": {
                    'billno': $scope.Settlement.billno,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'forsettlement': true,
                    'isimport': true
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SaveRequest = false;

                if (typeof (result) == 'undefined' || result.length == 0) {
                    $rootScope.Toaster.Error('Unsettled bill not found - ' + $scope.Settlement.billno);
                    $scope.Settlement.lastBillID = $scope.Settlement.billno;
                    $scope.lastbillno = $scope.Settlement.billno;
                    return;
                }
                var bill = result[0];
                if (bill.id == 0) {
                    return;
                }

                if ($scope.settlement_disablereturnbill && bill.billtype == 8 ) {
                    $rootScope.Toaster.Error("Return Bill can't be settled.");
                    return;
                }

                if (!$scope.AllowMultiCompany && $scope.Settlement.Billsettlement.length > 0) {
                    if ($scope.Settlement.Billsettlement[0].companyid != bill.companyid) {
                        $rootScope.Toaster.Error("Different company can't be settled together");
                        return;
                    }
                }

                if (typeof ($scope.Settlement.additionaldiscount) == 'undefined' || isNaN($scope.Settlement.additionaldiscount) || $scope.Settlement.additionaldiscount == null)
                    $scope.Settlement.additionaldiscount = 0;

                if (typeof ($scope.Settlement.billadditionaldiscount) == 'undefined' || isNaN($scope.Settlement.billadditionaldiscount) || $scope.Settlement.billadditionaldiscount == null)
                    $scope.Settlement.billadditionaldiscount = 0;

                $scope.Settlement.billmastercode = bill.billmasterid;
                $scope.Settlement.customername = bill.customername;
                $scope.Settlement.customerid = bill.customerid;
                $scope.Settlement.customermobileno = bill.phoneno;
                $scope.Settlement.rewardunit = bill.rewardunit;

                $scope.Settlement.tax += bill.tax;
                $scope.Settlement.discount += bill.discount;
                $scope.Settlement.billadditionaldiscount += bill.additionaldiscount;
                $scope.Settlement.billamount += bill.total;
                $scope.Settlement.receivable += bill.total;
                $scope.Settlement.billdate = bill.billdate;

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
                sbill.mode = bill.billtype;
                sbill.version = 1;
                sbill.tax = bill.tax;

                if ($scope.sale_canclaimsamebillrewards)
                    sbill.adjustablereward = bill.billattributes != null ? bill.billattributes.newreward : 0;
                sbill.discountreason = bill.discountrequestreason;
                sbill.billdate = bill.billdate;

                $scope.Settlement.Billsettlement.push(sbill);
                if (sbill.receivable < 0) {
                    $scope.Settlement.returnbillvalue += (sbill.receivable * -1);
                }
                if (typeof (callback) != 'undefined')
                    callback(bill, sbill);

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        };

        $scope.SentBills2Printer = function (bills) {

            if ($scope.ApplicationSettings.appsettings.settlement_discountbillprintmode == 1) {
                $rootScope.Handlers.SelectPrinter('', function (printdata) {
                    if (printdata == null)
                        return;

                    $.each(bills, function (k, v) {
                        $scope.Sent2Printer(v, printdata);
                    });
                });
            } else if ($scope.ApplicationSettings.appsettings.settlement_discountbillprintmode == 2) {
                $.each(bills, function (k, v) {
                    $scope.Sent2Printer(v);
                });
            }
        }

        $scope.Sent2Printer = function (result, printdata) {
            if (result.printdocument == null)
                return;

            var printerData = typeof (printdata) == 'undefined' ? localStorageService.get('PrinterData') : printdata;
            if (printerData == null || typeof (printerData) == 'undefined') {
                $rootScope.Toaster.Error("Please select the printer");
                return;
            }

            if (typeof (result.directpdf) != 'undefined' && result.directpdf) {
                $rootScope.Sent2Printer(result.printdocument, printerData, 'PDF');
            }
            else {
                $rootScope.Sent2Printer(result.printdocument, printerData);
            }
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
            //}).success(function(result) {

            //});
        }

        $scope.CheckDiscountPercentage = function () {
            if ($scope.Settlement.paymentinfo.discount_paid.indexOf('%') > 0) {
                var discountpercentage = $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid.replace('%', ''));
                if (discountpercentage > 0) {
                    $scope.Settlement.paymentinfo.discount_paid = ($scope.GetNumber($scope.Settlement.receivable) * discountpercentage / 100).round($scope.system_salesroundingdigit);
                    $scope.PaidChanged('DISCNT');
                }
            }
        }

        $scope.TouchPayment = function (mode) {
            switch (mode) {
                case 'CARD':
                    $scope.Settlement.paymentinfo.card_paid = $scope.GetNumber($scope.Settlement.receivable);
                    break;
                case 'UPI':
                    $scope.Settlement.paymentinfo.upi_paid = $scope.GetNumber($scope.Settlement.receivable);
                    break;
                case 'CREDIT':
                    $scope.Settlement.paymentinfo.credit_paid = $scope.GetNumber($scope.Settlement.receivable);
                    break;
                case 'VOUCHER':
                    $scope.SearchVoucher(function () {
                        if ($scope.Settlement.balanceamount<=0)
                            $scope.Save();
                    });
                    return;
                    break;
            }
            $scope.PaidChanged();
            $scope.Save();
        }

        $scope.Save = function (callback, validate) {
            if ((typeof (validate) == 'undefined' || validate == null) && $scope.Settlement.balanceamount > 0) {
                $rootScope.Toaster.Error("Balance -" + $scope.Settlement.balanceamount);
                return;
            }

            if (validate && $scope.Settlement.balanceamount > 0) {
                $rootScope.Toaster.Error("Balance -" + $scope.Settlement.balanceamount);
                return;
            }

            if (validate && $scope.Settlement.paymentinfo.discount_enable &&
                $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid) > 0 &&
                ($scope.Settlement.paymentinfo.discount_approvedBy <= 0)) {
                $rootScope.Toaster.Error("Fill necessary information for discount");
                $scope.setFocusdiscountpaid();

                //|| $scope.Settlement.paymentinfo.discount_info == null || $scope.Settlement.paymentinfo.discount_info.length < 3
                return;
            } else {
                $scope.Settlement.additionaldiscount = $scope.Settlement.paymentinfo.discount_paid;
            }

            if ($scope.GetNumber($scope.Settlement.receivable) > 0 &&
                $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid) > 0) {

                if ($scope.Settlement.paymentinfo.discount_byotp == 'undefined' ||
                    $scope.Settlement.paymentinfo.discount_byotp == null ||
                    !$scope.Settlement.paymentinfo.discount_byotp) {
                    var allowddiscount = ($scope.GetNumber($scope.Settlement.receivable) * $scope.GetNumber($rootScope.UserInfo.AD) / 100).round($scope.system_salesroundingdigit);
                    if (allowddiscount < $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid)) {
                        $rootScope.Toaster.Error("Maximum allowed discount " + allowddiscount);
                        $scope.setFocusdiscountpaid();
                        return;
                    }
                }
            }

            if ($scope.Settlement.balanceamount <= 0) {
                if ($scope.GetNumber($scope.Settlement.returnbillvalue) > 0
                    && $scope.GetNumber($scope.Settlement.paymentinfo.creditnote_refund) > 0) {
                    $scope.Settlement.paymentinfo.cash_refund = 0;
                    $scope.Settlement.paymentinfo.cash_return = 0;
                }
                else if ($scope.GetNumber($scope.Settlement.returnbillvalue) > 0) {
                    $scope.Settlement.paymentinfo.cash_refund = $scope.Settlement.balanceamount * -1;
                    $scope.Settlement.paymentinfo.cash_return = $scope.Settlement.balanceamount * -1;
                }
                else {
                    $scope.Settlement.paymentinfo.cash_return = $scope.Settlement.balanceamount * -1;
                    $scope.Settlement.paymentinfo.cash_refund = 0;
                }
            }

            if ($scope.Settlement.intvendor &&
                $scope.Settlement.paymentinfo &&
                $scope.Settlement.paymentinfo.deviceresult &&
                $scope.Settlement.paymentinfo.deviceresult != null &&
                $scope.Settlement.intvendor > 0) {
                validate = false;
            }


            if ($scope.GetNumber($scope.Settlement.paymentinfo.card_paid) > $scope.GetNumber($scope.Settlement.receivable)
                && $scope.GetNumber($scope.Settlement.receivable) > 0) {
                $rootScope.Toaster.Error("Card payment can't exceed the bill value");
                return;
            }

            if ($scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) > $scope.GetNumber($scope.Settlement.receivable)
                && $scope.GetNumber($scope.Settlement.receivable) > 0) {
                $rootScope.Toaster.Error("UPI payment can't exceed the bill value");
                return;
            }

            if (($scope.GetNumber($scope.Settlement.paymentinfo.card_paid) > 0 || $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) > 0)
                && ($scope.GetNumber($scope.Settlement.paymentinfo.card_paid) + $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid)) > $scope.GetNumber($scope.Settlement.receivable)) {
                $rootScope.Toaster.Error("Card/UPI payment can't exceed the bill value");
                return;
            }

            if (validate &&
                $scope.GetNumber($scope.Settlement.paymentinfo.loan_paid) > $scope.GetNumber($scope.Settlement.receivable)
                && $scope.GetNumber($scope.Settlement.receivable) > 0) {
                $rootScope.Toaster.Error("Loan payment can't exceed the bill value");
                return;
            }

            if (validate && $scope.GetNumber($scope.Settlement.paymentinfo.loan_paid) > 0) {
                if ($scope.Settlement.customerid == 0) {
                    $rootScope.Toaster.Error("Loan can be provided, only for customer bills");
                    return;
                }

                if (typeof ($scope.Settlement.paymentinfo.loanproviderid) == 'undefined' || $scope.Settlement.paymentinfo.loanproviderid <= 0) {
                    $rootScope.Toaster.Error("Please select the loan provider");
                    return;
                }
                else {
                    var provider = $scope.LoanProviderList.find(function (item) {
                        return item.id == $scope.Settlement.paymentinfo.loanproviderid;
                    });
                    $scope.Settlement.paymentinfo.loanprovidername = provider.name;
                }

                if (typeof ($scope.Settlement.paymentinfo.loan_reference) == 'undefined' || $scope.Settlement.paymentinfo.loan_reference.length <= 0) {
                    $rootScope.Toaster.Error("Please enter the loan reference no.");
                    return;
                }
            }

            if (validate && $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) > ($scope.GetNumber($scope.Settlement.receivable) + 2000)
                && $scope.GetNumber($scope.Settlement.receivable) > 0) {
                $rootScope.Toaster.Error("Cash payment can't exceed the bill value");
                return;
            }


            if (validate && $scope.GetNumber($scope.Settlement.paymentinfo.loan_paid) > 0 &&
                $scope.Settlement.paymentinfo.loanproviderid <= 0) {
                $rootScope.Toaster.Error("Please select the loan provider.");
                return;
            }


            if (validate && $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid) > 0 &&
                $scope.Settlement.paymentinfo.credit_approved <= 0) {
                $rootScope.Toaster.Error("Please select the approver.");
                return;
            }

            if (($scope.int_finance || validate) && $scope.GetNumber($scope.Settlement.paymentinfo.card_paid) > 0 &&
                $scope.Settlement.paymentinfo.card_bankid <= 0) {
                $rootScope.Toaster.Error("Please select the bank.");
                return;
            }

            if ($scope.GetNumber($scope.Settlement.paymentinfo.card_paid) > 0 &&
                $scope.GetNumber($scope.Settlement.paymentinfo.card_bankid) <= 0 && 
                $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) <= 0 && 
                $scope.GetNumber($scope.Settlement.paymentinfo.upiproviderid) > 0) {
                $rootScope.Toaster.Error("Please select the bank.");
                return;
            }

            if ($scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) > 0 &&
                $scope.GetNumber($scope.Settlement.paymentinfo.upiproviderid) <= 0 && 
                $scope.GetNumber($scope.Settlement.paymentinfo.card_paid) <= 0 && 
                $scope.GetNumber($scope.Settlement.paymentinfo.card_bankid) > 0) {
                $rootScope.Toaster.Error("Please select the UPI Provider.");
                return;
            }

            if (($scope.int_finance || validate) && $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) > 0 &&
                $scope.Settlement.paymentinfo.upiproviderid <= 0) {
                $rootScope.Toaster.Error("Please select the UPI Provider.");
                return;
            }

            var computedpaid = $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.card_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.loan_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.voucher_paid) -
                $scope.GetNumber($scope.Settlement.paymentinfo.cash_return);

            var cashpaid = $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid);
            var creditpaid = $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid);
            var topaid = cashpaid + creditpaid;

            if (validate && computedpaid != $scope.GetNumber($scope.Settlement.receivable)) {
                $rootScope.Toaster.Error("Paid amount mismatch, refresh the page, re-enter the values (" + computedpaid + ")");
                return;
            }

            if ($scope.Settlement.paidamount == 0
                && $scope.GetNumber($scope.Settlement.refundamount) == 0
                && $scope.Settlement.balanceamount != 0) return;

            if ($scope.settlement_disableadjustment && $scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) > 0) {
                $rootScope.Toaster.Error("Settlement Adjustment not allowed.");
                return;
            }

            if ($scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) < 0) {
                $rootScope.Toaster.Error("Adjustment can't be negative");
                return;
            }
            if ($scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) > $scope.GetNumber($scope.settlement_maxadjustment) &&
                $scope.GetNumber($scope.Settlement.receivable) < 1000) {
                $rootScope.Toaster.Error("Adjustment can't exceed 5 rupees, try to issue discount");
                return;
            }

            if ($scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) > 10 &&
                $scope.GetNumber($scope.Settlement.receivable) >= 1000) {
                $rootScope.Toaster.Error("Adjustment can't exceed 10 rupees, try to issue discount");
                return;
            }

            if ($scope.Settlement.Billsettlement.length == 0) {
                $rootScope.Toaster.Error("Fill necessary information for settlement");
                return;
            }

            if (validate && $scope.Settlement.paymentinfo.card_paid > 0) {
                if ($scope.system_swippingmachine) {
                    if (typeof ($scope.Settlement.paymentinfo.card_machineid) == 'undefined') {
                        $rootScope.Toaster.Error("Please select the bank/swipping machine.");
                        return;
                    }
                    var swipmachine = $scope.SwippingList.find(function (item) {
                        return item.id == $scope.Settlement.paymentinfo.card_machineid;
                    });
                    if (typeof (swipmachine) != 'undefined' && swipmachine != null)
                        $scope.Settlement.paymentinfo.card_bankid = swipmachine.additionalinfo.parentid;
                }

                if (typeof ($scope.Settlement.paymentinfo.card_bankid) == 'undefined' || $scope.Settlement.paymentinfo.card_bankid <= 0) {
                    $rootScope.Toaster.Error("Please select the bank.");
                    return;
                }

                if ($scope.system_salecollectcarddata) {
                    if (typeof ($scope.Settlement.paymentinfo.card_no) == 'undefined' || $scope.Settlement.paymentinfo.card_no.length <= 0) {
                        $rootScope.Toaster.Error("Please enter card number.");
                        return;
                    }
                    if (typeof ($scope.Settlement.paymentinfo.card_typeid) == 'undefined' || $scope.Settlement.paymentinfo.card_typeid <= 0) {
                        $rootScope.Toaster.Error("Please select the card type/bank.");
                        return;
                    }
                }
            }

            if ($scope.Settlement.paymentinfo.referral) {
                if ($scope.Settlement.paymentinfo.referralemployee) {
                    $scope.Settlement.paymentinfo.referralagentid = 0;
                    if (typeof ($scope.Settlement.paymentinfo.referralemployeeid) == 'undefined' ||
                        $scope.Settlement.paymentinfo.referralemployeeid == null ||
                        $scope.Settlement.paymentinfo.referralemployeeid < 0) {
                        $rootScope.Toaster.Error("Please select referral employee.");
                        return;
                    }
                } else if ($scope.Settlement.paymentinfo.referralagent) {
                    $scope.Settlement.paymentinfo.referralemployeeid = 0;
                    if (typeof ($scope.Settlement.paymentinfo.referralagentid) == 'undefined' ||
                        $scope.Settlement.paymentinfo.referralagentid == null ||
                        $scope.Settlement.paymentinfo.referralagentid < 0) {
                        $rootScope.Toaster.Error("Please select referral agent.");
                        return;
                    }
                }
            }

            if ($scope.GetNumber($scope.Settlement.receivable) < 0
                && $scope.GetNumber($scope.Settlement.paymentinfo.cash_return) > 0
                && Math.abs($scope.GetNumber($scope.Settlement.receivable)) < Math.abs($scope.GetNumber($scope.Settlement.paymentinfo.cash_return))) {
                $rootScope.Toaster.Error("Settlement Refund Can't be Morethan the Bill Return Value.");
                return;
            }

            if ($scope.GetNumber($scope.Settlement.receivable) < 0
                && $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) < 0)
            {
                $rootScope.Toaster.Error("Remove cash paid amount and use 'R' for return refund.");
                return;
            }



            $scope.Settlement.floorid = $rootScope.LocationInfo.floorid;
            $scope.Settlement.floorname = $scope.floorname;
            $scope.Settlement.counterid = $rootScope.LocationInfo.counter.id;
            $scope.Settlement.locationid = $rootScope.LocationInfo.location.id;
            $scope.Settlement.countername = $scope.CounterName;
            $scope.Settlement.paymentinfo.voucherinformation = $scope.voucherlist;
            $scope.Settlement.paymentinfo.return_dinamination = '';
            if ($scope.IsAutoSettlement) {
                $scope.closeThisDialog($scope.Settlement);
                return;
            }

            var request = {
                "Item": $scope.Settlement
            }

            //$scope.ErrorMessage = "Saving...";
            $http({
                method: 'POST',
                url: "BillsettlementmasterService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    //$scope.ErrorMessage = "Successfully saved";
                    //$scope.ErrorStyle = "success";
                    $rootScope.Toaster.Info("Saved Successfully with Settlement No : " + result.code);
                    $scope.lastMasterBillID = result.id;
                    $scope.lastbillmastercode = result.code;
                    $scope.lastbillno = result.Billsettlement[0].billno;
                    $scope.lastrefundamount = $scope.GetNumber($scope.Settlement.paymentinfo.cash_return);
                    $scope.Sent2Printer(result);

                    if (result.discountedbills != null && result.discountedbills.length > 0)
                        $scope.SentBills2Printer(result.discountedbills);

                    $scope.CreateNew();
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
                if (event) event.preventDefault();
                $timeout(function () {
                    $scope.setFocusautocoding();
                }, 10);

            }).error(function (result) {
                $scope.ErrorMessage = result;
            });
        }

        $scope.ShowUpdateCustomerDialog = function (bill) {

            var dialog = ngDialog.open({
                template: 'UpdateCustomer',
                data: [$scope, bill],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.customernewid = 0;
                    $scope.ParentScope = $scope.ngDialogData[0];
                    $scope.SelectedBill = $scope.ngDialogData[1];
                    $scope.Search = function () {
                        var request = {
                            "Item": {
                                'name': $scope.name, 'mobileno': $scope.contactno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length > 0) {
                                $scope.customernewid = result[0].id;
                                $scope.name = result[0].name;
                            }
                        });
                    };

                    $scope.UpdateCreditBill = function () {
                        var request = {
                            "Item": {
                                'id': $scope.SelectedBill.billid,
                                'customerid': $scope.customernewid,
                                'customername': $scope.name,
                                'phoneno': $scope.contactno
                            },
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "BillService/UpdateBillasCredit",
                            data: srequest
                        }).success(function (result) {

                            if (result != null && result.id > 0) {
                                $rootScope.Toaster.Info("Updated Successfully.!");
                                $scope.ParentScope.ManageDeleteItem('Delete', bill);
                                $scope.closeThisDialog(null);
                            }
                            else {
                                $rootScope.Toaster.Info("Error - " + result.ErrorMessage);
                            }
                        }).error(function (result) {
                        });
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        //$scope.UpdateCustomer();
                    }
                }
            });
        }

        $scope.ManageCredit = function (action, bill) {

            if (bill.customerid == null || bill.customerid == 'undefined' || bill.customerid == 0) {
                $scope.ShowUpdateCustomerDialog(bill);
                return;
            }

            $.confirm({
                title: 'CREDIT',
                content: 'Do you want to convert the cash bill into Credit bill?',
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

                            var request = {
                                "Item": { 'id': bill.billid, 'customerid': bill.customerid },
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "BillService/UpdateBillasCredit",
                                data: srequest
                            }).success(function (result) {

                                if (result != null && result.id > 0) {
                                    $rootScope.Toaster.Info("Saved Successfully.!");
                                    $scope.ManageDeleteItem('Delete', bill);
                                }
                                else {
                                    $rootScope.Toaster.Info("Error - " + result.ErrorMessage);
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
        };

        $scope.ManageDeleteItem = function (action, bill) {
            $scope.Settlement.Billsettlement.splice($scope.Settlement.Billsettlement.indexOf(bill), 1);

            if (bill.receivable < 0)
                $scope.Settlement.returnbillvalue -= (bill.receivable * -1);

            if (bill.billtype == 1 || bill.iscreditbill) {
                $scope.Settlement.paymentinfo.credit_paid = $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid);
                $scope.Settlement.paymentinfo.credit_paid -= bill.total;
                $scope.Settlement.paymentinfo.has_credit = $scope.Settlement.paymentinfo.credit_paid > 0;
            };

            $scope.Settlement.tax -= bill.tax;
            $scope.Settlement.discount -= bill.discount;
            $scope.Settlement.billamount -= bill.receivable;
            $scope.Settlement.receivable -= bill.receivable;
            $scope.Settlement.billadditionaldiscount -= bill.billadditionaldiscount;
            $scope.CashChanged('0');
            $scope.autoCoding = '';
            $scope.Settlement.paymentinfo.cash_dinamination = '';
        };

        $scope.ManageSearchItem = function (action, item) {
            $scope.voucherlist.splice($scope.voucherlist.indexOf(item), 1);
            $scope.Settlement.paymentinfo.voucher_paid = 0;
            $.each($scope.voucherlist, function (k, v) {
                $scope.Settlement.paymentinfo.voucher_paid += v.amount;
            });
            $scope.PaidChanged();
        }
        $scope.SaveCustomer = function () {
            var request = {
                "Item": $scope.LoayaltyCutomer
            }

            $http({
                method: 'POST',
                url: "CustomerService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                if (result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                }

            }).error(function (result) {
                $scope.ErrorMessage = result;
            });
        }

        var letters = /^[0-9a-zA-Z]+$/;
        var formula = /^[0-9]+$/;
        $scope.CashChanged = function (cash) {
            $scope.Settlement.paymentinfo.adjustment_paid = 0;
            $scope.Settlement.paymentinfo.adjustment_enable = false;

            var value = 0;
            if (typeof (cash) == 'string' && cash.length > 0) {
                try {
                    value = eval(cash);

                    if (typeof (value) == 'undefined' || value == null || typeof (value) != 'number' || isNaN(value)) {
                        vaule = 0;
                        $scope.Settlement.paymentinfo.cash_paid = '';
                        $scope.Settlement.paymentinfo.cash_dinamination = '';
                        $scope.Settlement.paymentinfo.cash_return = 0;
                    }
                } catch (exp) {
                }
            }

            if (value == 0)
                cash = 0;

            $scope.Settlement.paymentinfo.cash_paid = $scope.GetNumber(cash) == 0 ? '' : cash;
            //$scope.Settlement.paymentinfo.cash_paid = value > 0 ? value : '';

            $scope.Settlement.paidamount = $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.card_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.loan_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.voucher_paid) +
                $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid);

            $scope.Settlement.paidamount = $scope.GetNumber($scope.Settlement.paidamount);
            $scope.Settlement.balanceamount = $scope.Settlement.receivable - $scope.Settlement.paidamount;

            if (Math.abs($scope.Settlement.balanceamount) < 1)
                $scope.Settlement.balanceamount = 0;

            if ($scope.Settlement.balanceamount < 0) {
                if ($scope.GetNumber($scope.Settlement.returnbillvalue) > 0) {
                    $scope.Settlement.paymentinfo.cash_refund = $scope.Settlement.balanceamount * -1;
                    $scope.Settlement.paymentinfo.cash_return = $scope.Settlement.balanceamount * -1;
                }
                else {
                    $scope.Settlement.paymentinfo.cash_return = $scope.Settlement.balanceamount * -1;
                    $scope.Settlement.paymentinfo.cash_refund = 0;
                }
                $scope.Settlement.paymentinfo.return_dinamination = $scope.GetPossibleDinamination($scope.Settlement.paymentinfo.cash_return);

            }
            else {
                $scope.Settlement.paymentinfo.cash_return = 0;
                $scope.Settlement.paymentinfo.cash_refund = 0;
                $scope.Settlement.paymentinfo.return_dinamination = '';
            }

            return value != 0;
        };

        $scope.excludeDinamination = [];

        $scope.GetPossibleDinamination = function (value, exclude) {
            var dinaminationMax = [2000, 500, 200, 100, 50];
            var dinaminationMin = [20, 10, 5, 2, 1];
            var values = [];
            var maxiteration = 3;

            if (typeof (exclude) != 'undefined') {
                $scope.excludeDinamination.push(exclude);
            } else {
                $scope.excludeDinamination = [];
            }

            if ($scope.excludeDinamination.length > 0) {
                $.each($scope.excludeDinamination, function (k, v) {
                    if (dinaminationMax.indexOf(v) >= 0) {
                        dinaminationMax.splice(dinaminationMax.indexOf(v), 1);
                    } else if (dinaminationMin.indexOf(v) >= 0) {
                        dinaminationMin.splice(dinaminationMin.indexOf(v), 1);
                    }
                })
            }

            while (value > 0 && maxiteration > 0) {
                $.each(dinaminationMax, function (k, v) {
                    if (v <= value) {
                        var multiplier = Math.floor(value / v);
                        value -= (v * multiplier);
                        values.push({ 'd': v, 'm': multiplier });
                    }
                });
                $.each(dinaminationMin, function (k, v) {
                    if (v <= value) {
                        var multiplier = Math.floor(value / v);
                        value -= (v * multiplier);
                        values.push({ 'd': v, 'm': multiplier });
                    }
                });
                maxiteration--;
            }
            return values;
        };

        $scope.DinaminationChanged = function (item) {
            var val = 0;
            $.each($scope.Settlement.paymentinfo.return_dinamination, function (k, v) {
                val += (v.m * v.d);
            });
            val = $scope.Settlement.paymentinfo.cash_return - val;
            if (val > 0) {
                var din = $scope.GetPossibleDinamination(val, item.d);
                $.each(din, function (k, v) {
                    var exist = $scope.Settlement.paymentinfo.return_dinamination.find(function (item) {
                        return item.d == v.d;
                    });
                    if (exist != null) {
                        exist.m += v.m;
                    }
                    else {
                        $scope.Settlement.paymentinfo.return_dinamination.push(v);
                    }
                });
            }
            else {
                val = $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) - $scope.Settlement.receivable;
                if ($scope.GetNumber($scope.Settlement.returnbillvalue) > 0) {
                    $scope.Settlement.paymentinfo.cash_refund = val;
                    $scope.Settlement.paymentinfo.cash_return = val;
                }
                else {
                    $scope.Settlement.paymentinfo.cash_return = val;
                    $scope.Settlement.paymentinfo.cash_refund = 0;
                }
                $scope.Settlement.paymentinfo.return_dinamination = $scope.GetPossibleDinamination($scope.Settlement.paymentinfo.cash_return);
            }
        };


        $scope.ManageDiscountItem = function () {


            var dialog = ngDialog.open({
                template: 'SpecialDiscount',
                data: {
                    'EmployeeList': $scope.EmployeeList, 'ParentScope': $scope
                },
                width: '600px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Settlement = $scope.ngDialogData.ParentScope.Settlement;
                    $scope.EmployeeList = $scope.ngDialogData.EmployeeList;
                    $scope.CanApprove = $rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view;
                    $scope.AD = parseFloat($rootScope.UserInfo.AD);
                    $scope.totalreceivable = $scope.Settlement.billamount;
                    $scope.DiscountValue = 0;
                    $timeout(function () {
                        $scope.setFocusdiscountpercent();
                    }, 50);
                    $scope.discountvaluechanged = function () {
                        $scope.DiscountValue = parseFloat($scope.discount_paid);
                        if (isNaN($scope.DiscountValue)) {
                            $scope.DiscountValue = 0;
                            $scope.discount_paid = '';
                            $scope.discount_paidpercentage = '';
                        }
                        if ($scope.DiscountValue > 0) {
                            $scope.discount_paidpercentage = ($scope.DiscountValue / $scope.GetNumber($scope.totalreceivable) * 100).round(2);
                        }
                        if ($scope.GetNumber($scope.discount_paidpercentage) > $scope.GetNumber($scope.AD))
                            $scope.CanApprove = false;

                    }
                    $scope.discountchanged = function () {
                        var percent = parseFloat($scope.discount_paidpercentage);
                        if (isNaN(percent)) {
                            $scope.discount_paidpercentage = '';
                            percent = 0;
                        }
                        $scope.discount_paid = ($scope.totalreceivable * percent / 100).toFixed(0);
                        $scope.DiscountValue = parseFloat($scope.discount_paid);

                        if ($scope.GetNumber($scope.discount_paidpercentage) > $scope.GetNumber($scope.AD))
                            $scope.CanApprove = false;
                    }

                    $scope.RequestSMS = function () {
                        $scope.totalreceivable = $rootScope.RoundRupee($scope.totalreceivable);
                        var request = {
                            "Item": {
                                'code': $scope.EmployeeCode,
                                'locationid': $rootScope.LocationInfo.location.id,
                                'permission': 'CANAPPROVEDISCOUNT',
                                'source':
                                {
                                    'receivable': $scope.totalreceivable - $scope.DiscountValue, 'discount': $scope.DiscountValue,
                                    'billno': $scope.ParentScope.Settlement.Billsettlement[0].billno,
                                    'billdate': $scope.ParentScope.Settlement.Billsettlement[0].billdate,
                                    'discountrequestreason': $scope.discount_info
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
                        if (!$scope.CanApprove) {
                            $rootScope.Toaster.Notice("Please enter employee code to get the passcode");
                            return;
                        }
                        if ($scope.GetNumber($scope.discountpercentage) == isNaN) {
                            $rootScope.Toaster.Notice("Please enter valid discount percentage ");
                            return;
                        }

                        if ($scope.GetNumber($scope.discountpercentage) > $scope.AD) {
                            $rootScope.Toaster.Error("Your allowed discount maximum @" + $scope.AD + '%');
                            return;
                        }
                        $scope.closeThisDialog({
                            'discount_paid': $scope.discount_paid,
                            'discount_info': $scope.discount_info,
                            'discount_approvedBy': $scope.discount_approvedBy,
                            'status': 'approved'
                        });
                    };

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                    $scope.Settlement.paymentinfo.discount_paid = data.value.discount_paid;
                    $scope.Settlement.paymentinfo.discount_info = data.value.discount_info;
                    $scope.Settlement.paymentinfo.discount_approvedBy = data.value.discount_approvedBy;
                    $scope.Settlement.paymentinfo.discount_byotp = data.value.status == 'approved' ? true : false;
                    $scope.PaidChanged('DISCNT');
                }
                $scope.setFocuscashpaid();
            });
        };

        $scope.Refund = function () {
            $scope.Settlement.refundamount = ($scope.Settlement.balanceamount * -1).round($scope.system_salesroundingdigit);
            $scope.Settlement.paymentinfo.cash_refund = $scope.Settlement.refundamount;
            //$scope.lastrefundamount = $scope.Settlement.refundamount;
            $scope.Save(function () {
                $scope.autoCoding.code = '';
                $scope.setFocusautocoding();
            }, false);
        };

        $scope.RefundWithCreditNote = function () {
            if (!$scope.system_supportcustcreditnote) {
                $rootScope.Toaster.Error("Issue of customer credit note not enabled., Please contact system support.");
                $scope.autoCoding.code = '';
                return;
            }

            if (typeof ($rootScope.UserInfo.p_SUPPORTCREDITNOTESETTLEMENT) == 'undefined' || !$rootScope.UserInfo.p_SUPPORTCREDITNOTESETTLEMENT.view) {
                $rootScope.Toaster.Error("Don't have permission to issue credit note");
                $scope.autoCoding.code = '';
                return;
            }

            $scope.Settlement.refundamount = ($scope.Settlement.balanceamount * -1).round($scope.system_salesroundingdigit);
            $scope.Settlement.paymentinfo.creditnote_refund = $scope.Settlement.refundamount;
            $scope.Save(function () {
                $scope.autoCoding.code = '';
                $scope.setFocusautocoding();
            }, false);
        };

        $scope.Referral = function (token) {
            $scope.Floors = restService.Get('StoretransactionService/ReferralToken/' + $rootScope.LocationInfo.location.id + '/' + token).then(function (result) {
                if (result == null) {
                    $rootScope.Toaster.Error("Given token is not valid!");
                }
                else if (result.settlementid > 0) {
                    $rootScope.Toaster.Error("Given token already consumed!");
                }
                else {
                    $scope.Settlement.paymentinfo.referral = token;
                    $scope.Settlement.paymentinfo.referralemployee = result.mode == 1;
                    $scope.Settlement.paymentinfo.referralagent = result.mode == 2;
                }
            });
        }

        $scope.SearchAutoCoding = function () {
            if ($scope.autoCoding.code == null || $scope.autoCoding.code == "") {
                if ($scope.Settlement.Billsettlement.length > 0 && $scope.Settlement.balanceamount > 0) {
                    var isreturn = false;
                    if ($scope.Settlement.balanceamount <= $scope.GetNumber($scope.settlement_maxadjustment)
                        && !$scope.settlement_disableadjustment) {
                        $.each($scope.Settlement.Billsettlement, function (k, v) {
                            if (v.receivable < 0) {
                                $scope.Settlement.paymentinfo.adjustment_paid = $scope.Settlement.balanceamount;
                                $scope.Settlement.paymentinfo.adjustment_enable = true;
                                $scope.Settlement.paidamount += $scope.Settlement.balanceamount;
                                $scope.Settlement.balanceamount = $scope.Settlement.receivable - $scope.Settlement.paidamount;
                                $scope.setFocusadjustment_paid();
                                isreturn = true;
                            }
                        });
                    }
                    else
                        $scope.setFocuscashpaid();

                    if (isreturn) return;
                }
                else if ($scope.Settlement.balanceamount == 0 && $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid) > 0) {
                    $scope.setFocusapprovedby();
                    return;
                }
            }

            if ($scope.autoCoding.code.length == 10 &&
                $scope.autoCoding.code.startsWith('$') &&
                $scope.autoCoding.code.endsWith('$')) {
                $scope.Referral($scope.autoCoding.code);
                $scope.autoCoding.code = '';
                return;
            }


            if ($scope.autoCoding.code == "00" && $scope.Settlement.Billsettlement.length > 0) {
                if ($scope.Settlement.balanceamount > 0) {
                    $scope.Settlement.paymentinfo.cash_paid = $scope.GetNumber($scope.Settlement.paymentinfo.cash_paid) + $scope.Settlement.balanceamount;
                    $scope.Settlement.paidamount += $scope.Settlement.balanceamount;
                    $scope.Settlement.balanceamount = 0;
                }
                $scope.autoCoding.code = "0";
            } else if ($scope.autoCoding.code == "01" && $scope.Settlement.Billsettlement.length > 0) {
                if ($scope.Settlement.balanceamount > 0) {
                    $scope.Settlement.paymentinfo.cash_paid = 0;
                    $scope.Settlement.paymentinfo.card_paid += $scope.Settlement.billamount;
                    $scope.Settlement.paymentinfo.card_bankid = $scope.BankList[0].id;
                    $scope.Settlement.paidamount += $scope.Settlement.billamount;
                    $scope.Settlement.balanceamount = 0;
                }
                $scope.autoCoding.code = "0";
            } else if ($scope.autoCoding.code == "02" && $scope.Settlement.Billsettlement.length > 0) {
                if ($scope.Settlement.balanceamount > 0) {
                    $scope.Settlement.paymentinfo.cash_paid = 0;
                    $scope.Settlement.paymentinfo.card_paid = 0;
                    $scope.Settlement.paymentinfo.upi_paid += $scope.Settlement.billamount;
                    $scope.Settlement.paymentinfo.upiproviderid = $scope.UPIProviderList[0].id;
                    $scope.Settlement.paidamount += $scope.Settlement.billamount;
                    $scope.Settlement.balanceamount = 0;
                }
                $scope.autoCoding.code = "0";
            }


            if ($scope.autoCoding.code == "0") {
                if ($scope.Settlement.Billsettlement.length > 0) {

                    if ($scope.Settlement.balanceamount <= 0) {
                        if ($scope.GetNumber($scope.Settlement.paymentinfo.credit_paid) > 0) {
                            if ($scope.Settlement.paymentinfo.credit_approved > 0)
                                $scope.Save(null, false);
                            else
                                $scope.setFocusapprovedby();
                        }
                        else {
                            $scope.Save(null, false);
                        }
                    }
                    else {
                        $scope.setFocuscardpaid();
                    }
                }
                else {
                    $scope.autoCoding.code = '';
                    $scope.setFocuscardpaid();
                }
                return;
            }

            if ($scope.autoCoding.code == "" && $scope.Settlement.balanceamount > 0) {
                if ($scope.PaymentIntegration != null &&
                    $scope.PaymentIntegration.Integrater &&
                    $scope.PaymentIntegration.Integrater != null) {
                    $scope['setFocus' + $scope.PaymentIntegration.Integrater.focusid]();
                }
                else {
                    if ($scope.Settlement.paymentinfo.discount_enable)
                        $scope.setFocusdiscountpaid();
                    else
                        $scope.setFocuscardpaid();
                }
                return;
            }

            if ($scope.autoCoding.code == "P" || $scope.autoCoding.code == "p") {
                if ($scope.Settlement.id > 0 || Settlement.lastBillID != '') {
                    $scope.Print();
                }
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code == "-"
                && $scope.Settlement.Billsettlement.length > 0) {
                $scope.ManageDeleteItem('Delete', $scope.Settlement.Billsettlement[0]);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code == "*") {
                while ($scope.Settlement.Billsettlement.length > 0)
                    $scope.ManageDeleteItem('Delete', $scope.Settlement.Billsettlement[0]);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code == "N" || $scope.autoCoding.code == "n") {
                $scope.CreateNew();
                return;
            }

            if (($scope.autoCoding.code == "R" || $scope.autoCoding.code == "r") &&
                $scope.Settlement.balanceamount < 0) {

                if ($scope.system_posdisablerefund) {
                    $rootScope.Toaster.Error("Refund disabled, please contact your manager.");
                    $scope.autoCoding.code = '';
                    return;
                }

                $scope.Refund();
                return;
            }

            if (($scope.autoCoding.code == "RC" || $scope.autoCoding.code == "rc") &&
                $scope.Settlement.balanceamount < 0) {
                $scope.RefundWithCreditNote();
                return;
            }

            if (($scope.autoCoding.code == "D" || $scope.autoCoding.code == "d")) {
                if (!$scope.ApplicationSettings.appsettings.settlement_additionaldiscount) {
                    $rootScope.Toaster.Error("Settlement discount disabled accross the organization");
                    $scope.autoCoding.code = '';
                    return;
                }

                //if (!$scope.sale_requestpasscodetosms) {
                //    if (typeof ($rootScope.UserInfo.p_CANAPPROVEDISCOUNT) == 'undefined' || !$rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view) {
                //        $rootScope.Toaster.Error("you don't have discount permission");
                //        $scope.autoCoding.code = '';
                //        return;
                //    }
                //}

                if ($scope.Settlement.paymentinfo.discount_enable) {
                    $scope.Settlement.paymentinfo.discount_enable = false;
                    $scope.Settlement.paymentinfo.discount_paid = 0;
                    $scope.autoCoding.code = '';
                }
                else {
                    $scope.autoCoding.code = '';
                    $scope.Settlement.paymentinfo.discount_enable = true;
                    //var nopermission = (typeof ($rootScope.UserInfo.p_CANAPPROVEDISCOUNT) == 'undefined' || !$rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view)
                    var nopermission = (typeof ($rootScope.UserInfo.p_DISCOUNTONSETTLEMENT) == 'undefined' || !$rootScope.UserInfo.p_DISCOUNTONSETTLEMENT.view)
                    if (nopermission && $scope.sale_requestpasscodetosms) {
                        $scope.ManageDiscountItem();
                    }
                    else {
                        $timeout(function () {
                            $scope.setFocusdiscountpaid();
                        });
                    }
                }
                return;
            }


            //if ($scope.billprefix != null) {
            if ($scope.BillNoConfig.isbillno($scope.autoCoding.code.toUpperCase()))
            //if ($scope.autoCoding.code.startsWith($scope.billprefix))
            {
                $scope.Settlement.billno = $scope.autoCoding.code.toUpperCase();
                if ($scope.Settlement.billno.startsWith('#')) {
                    $scope.Settlement.billno = $scope.Settlement.billno.substring(1);
                }

                var existingbill = $scope.Settlement.Billsettlement.find(function (item) {
                    return item.billno == $scope.Settlement.billno;
                });

                if (typeof (existingbill) != 'undefined' && existingbill != null) {
                    $scope.Settlement.lastBillID = $scope.Settlement.billno;
                    $scope.autoCoding.code = '';
                    return;
                }


                $scope.SearchBill(function (bill) {
                    $scope.autoCoding.code = '';
                    if (bill.billtype == 1 || bill.iscreditbill) {
                        $scope.Settlement.paymentinfo.credit_paid = $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid);
                        $scope.Settlement.paymentinfo.credit_paid += bill.total;
                        $scope.Settlement.paymentinfo.has_credit = true;
                    };
                    $scope.PaidChanged();
                });
            }
            else if ($scope.BillNoConfig.issettlement($scope.autoCoding.code.toUpperCase()))
            //if ($scope.autoCoding.code.startsWith($scope.billprefix))
            {
                $scope.Settlement.billno = $scope.autoCoding.code.toUpperCase();

                var request = {
                    "Item": {
                        'billno': $scope.Settlement.billno,
                        'locationid': $rootScope.LocationInfo.location.id,
                        'forsettlement': true
                    },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);
                $scope.SaveRequest = true;
                $http({
                    method: 'POST',
                    url: "BillService/SearchMasterBill",
                    data: srequest
                }).success(function (results) {

                    $scope.SaveRequest = false;
                    if (!angular.isArray(results) || results.length == 0) {
                        $rootScope.Toaster.Error('Bill not available or bill already settiled.');
                        $scope.Settlement.lastMasterBillID = $scope.Settlement.billno;
                        return;
                    }

                    var bill = results[0];

                    if ($scope.Settlement.Billsettlement.length > 0) {
                        for (var i = 0; i < results.length; i++) {
                            bill = results[i];
                            var existingbill = $scope.Settlement.Billsettlement.find(function (item) {
                                return bill.id == item.billid;
                            });
                            if (typeof (existingbill) != 'undefined' && existingbill != null) {
                                $rootScope.Toaster.Error('Already bill available in the list.');
                                return;
                            }
                        }
                    }
                    else {
                        $scope.Settlement = (JSON.parse(JSON.stringify($scope.TemplateSettlement)));
                        $scope.Settlement.additionaldiscount = 0;
                        $scope.Settlement.billadditionaldiscount = 0;
                        $scope.Settlement.billmastercode = bill.billmasterid;
                        $scope.Settlement.customername = bill.customername;
                        $scope.Settlement.customerid = bill.customerid;
                        $scope.Settlement.customermobileno = bill.phoneno;
                        $scope.Settlement.rewardunit = bill.rewardunit;
                    }

                    for (var i = 0; i < results.length; i++) {
                        bill = results[i];
                        $scope.Settlement.tax += bill.tax;
                        $scope.Settlement.discount += bill.discount;
                        $scope.Settlement.billadditionaldiscount += bill.additionaldiscount;
                        $scope.Settlement.billamount += bill.total;
                        $scope.Settlement.receivable += bill.total;
                        $scope.Settlement.billdate = bill.billdate;

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
                        sbill.mode = bill.billtype;
                        sbill.version = 1;
                        sbill.tax = bill.tax;

                        if ($scope.sale_canclaimsamebillrewards)
                            sbill.adjustablereward = bill.billattributes != null ? bill.billattributes.newreward : 0;

                        sbill.discountreason = bill.discountrequestreason;
                        $scope.Settlement.Billsettlement.push(sbill);
                        if (sbill.receivable < 0) {
                            $scope.Settlement.returnbillvalue += (sbill.receivable * -1);
                        }
                        if (bill.billtype == 1 || bill.iscreditbill) {
                            $scope.Settlement.paymentinfo.credit_paid = $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid);
                            $scope.Settlement.paymentinfo.credit_paid += bill.total;
                            $scope.Settlement.paymentinfo.has_credit = true;
                        };
                    }
                    $scope.autoCoding.code = '';
                    $scope.PaidChanged();
                }).error(function (result) {
                    $scope.SaveRequest = false;
                });;
            }
            else if ($scope.autoCoding.code.length > 10) { //&& $scope.Settlement.paymentinfo.cash_dinamination != $scope.autoCoding.code
                $scope.Settlement.paymentinfo.cash_paid = 0;
                $scope.Settlement.paymentinfo.cash_dinamination = 0;
                if ($scope.autoCoding.code.match(letters)) {

                    $scope.vouchercode = $scope.autoCoding.code;
                    $scope.SearchVoucher(function (hasvoucher) {
                        if (hasvoucher)
                            $scope.autoCoding.code = '';
                    });
                }
            }
            else if ($scope.Settlement.balanceamount <= 0) {
                $scope.autoCoding.code = '';
                $scope.Save(null, false);
            }
            else if ($scope.Settlement.balanceamount <= $scope.GetNumber($scope.settlement_maxadjustment) && !$scope.settlement_disableadjustment) {
                $scope.Settlement.paymentinfo.adjustment_paid = $scope.Settlement.balanceamount;
                $scope.Settlement.paymentinfo.adjustment_enable = true;
                $scope.Settlement.paidamount += $scope.Settlement.balanceamount;
                $scope.Settlement.balanceamount = $scope.Settlement.receivable - $scope.Settlement.paidamount;
                $scope.autoCoding.code = '';
                $timeout(function () {
                    $scope.setFocusadjustment_paid();
                }, 50);
            }
            else {
                $scope.autoCoding.code = '';
                $scope.setFocuscardpaid();
            }
            //}
            //else {
            //    $scope.Settlement.billno = $scope.autoCoding.code;
            //    var existingbill = $scope.Settlement.Billsettlement.find(function (item) {
            //        return item.billno == $scope.autoCoding.code;
            //    });

            //    if (typeof (existingbill) != 'undefined' && existingbill != null) {
            //        $scope.autoCoding.code = '';
            //        return;
            //    }
            //    $scope.SearchBill(function (bill) {
            //        $scope.autoCoding.code = '';
            //        if (bill.billtype == 1) {
            //            $scope.Settlement.paymentinfo.credit_paid = $scope.GetNumber($scope.Settlement.paymentinfo.credit_paid);
            //            $scope.Settlement.paymentinfo.credit_paid += bill.total;
            //        };
            //        $scope.PaidChanged();
            //    });
            //}
        }


        $scope.PaidChanged = function (mode) {

            //if ($scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) > 5 || $scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) < 0) {
            //    $scope.Settlement.paymentinfo.adjustment_paid = '';
            //}
            if (mode == 'DISCNT') {
                if ($scope.Settlement.paymentinfo.discount_paid.toString().endsWith('%')) {
                    var discountpercentage = $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid.replace('%', ''));
                    if (discountpercentage > 0) {
                        $scope.Settlement.paymentinfo.discount_percentage = discountpercentage;
                        $scope.Settlement.paymentinfo.discount_paid = ($scope.GetNumber($scope.Settlement.receivable) * discountpercentage / 100).round($scope.system_salesroundingdigit);
                    }
                }
                else {
                    var discountvalue = $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid.replace('%', ''));
                    var discountpercentage = (discountvalue / $scope.GetNumber($scope.Settlement.receivable) * 100).round(2);
                    $scope.Settlement.paymentinfo.discount_percentage = discountpercentage;
                }

                if ($scope.GetNumber($scope.Settlement.receivable) > 0 &&
                    $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid) > 0) {

                    if ($scope.Settlement.paymentinfo.discount_byotp == 'undefined' ||
                        $scope.Settlement.paymentinfo.discount_byotp == null ||
                        !$scope.Settlement.paymentinfo.discount_byotp) {
                        var allowddiscount = ($scope.GetNumber($scope.Settlement.receivable) * $scope.GetNumber($rootScope.UserInfo.AD) / 100).round($scope.system_salesroundingdigit);
                        if (allowddiscount < $scope.GetNumber($scope.Settlement.paymentinfo.discount_paid)) {
                            $rootScope.Toaster.Error("Maximum allowed discount " + allowddiscount);
                            $scope.Settlement.paymentinfo.discount_paid = 0;
                            $scope.Settlement.paymentinfo.discount_percentage = 0;
                            $scope.Settlement.paidamount = 0;
                            $scope.setFocusdiscountpaid();
                            return;
                        }
                    }
                }
            }

            if (mode == 'CARD' && $scope.BankList.length == 1) {
                $scope.Settlement.paymentinfo.card_bankid = $scope.BankList[0].id;
            }

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

            //if ($scope.GetNumber($scope.Settlement.paymentinfo.adjustment_paid) == 0)
            //{
            //    $scope.Settlement.paymentinfo.adjustment_enable = false;
            //    $scope.Settlement.paymentinfo.adjustment = '';
            //}
            //else
            //if ($scope.Settlement.paymentinfo.adjustment_enable) {
            //    if ($scope.Settlement.balanceamount > 5 || $scope.Settlement.balanceamount <= 0) {
            //        //$scope.Settlement.paymentinfo.adjustment_enable = false;
            //        $scope.Settlement.paymentinfo.adjustment = '';
            //    }
            //    else {
            //        var adjustment = $scope.GetNumber($scope.Settlement.paymentinfo.adjustment);
            //        if (adjustment <= 0 || adjustment > 5) {
            //            $scope.Settlement.paymentinfo.adjustment = '';
            //        }
            //        else {
            //            $scope.Settlement.paidamount = $scope.GetNumber($scope.Settlement.paidamount) + adjustment;
            //            $scope.Settlement.balanceamount = $scope.Settlement.receivable - $scope.Settlement.paidamount;
            //        }

            //        //if ($scope.Settlement.balanceamount > 0 && $scope.Settlement.balanceamount <= 5) {
            //        //    $scope.Settlement.paymentinfo.adjustment = $scope.Settlement.balanceamount;
            //        //    $scope.Settlement.paidamount += $scope.Settlement.balanceamount;
            //        //    $scope.Settlement.balanceamount = $scope.Settlement.receivable - $scope.Settlement.paidamount;
            //        //}
            //        //else {
            //        //    $scope.Settlement.paymentinfo.adjustment_enable = false;
            //        //}
            //    }
            //}
        };

        $scope.TouchButton = function (action, key) {
            $scope.autoCoding.code = key;
            $scope.SearchAutoCoding();
        }

        $scope.checkloansettlement = function () {
            if ($scope.system_loansettlement) {
                $scope['setFocusloanpaid']();
            }
            else if ($scope.Settlement.paymentinfo.has_credit && $scope.Settlement.paymentinfo.credit_paid > 0) {
                $scope.checkbalanceandgo('autocoding');
            }
            else {
                $scope['setFocusautocoding']();
            }
        }

        $scope.checkmachineandbank = function () {
            if ($scope.GetNumber($scope.Settlement.paymentinfo.card_paid) <= 0) {
                $scope['setFocusupipaid']();
            }
            else {
                if ($scope.system_swippingmachine) {
                    $scope['setFocuscardmachineid']();
                }
                else {
                    $scope['setFocusbankid']();
                }
            }
        }

        $scope.checkbalanceandgo = function (nextfocus) {
            $scope.PaidChanged();

            if (nextfocus == 'cardno') {
                if ($scope.system_salecollectcarddata && $scope.GetNumber($scope.Settlement.paymentinfo.card_paid) > 0) {
                    nextfocus = 'cardno';
                    $scope['setFocus' + nextfocus]();
                    return;
                }
            }

            if (nextfocus == 'cardno') {
                if (!$scope.system_salecollectcarddata || $scope.GetNumber($scope.Settlement.paymentinfo.card_paid)) {
                    nextfocus = 'upipaid';
                }
            }

            if (nextfocus == 'upiprovider') {
                if ($scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) <= 0) {
                    nextfocus = 'creditpaid';
                }
                else {
                    $scope.setFocusupiprovider();
                    return;
                }
            }
            if (nextfocus == 'upicardno') {
                if ($scope.system_salecollectcarddata && $scope.GetNumber($scope.Settlement.paymentinfo.upi_paid) > 0) {
                    nextfocus = 'upicardno';
                    $scope['setFocus' + nextfocus]();
                    return;
                }
            }

            if (nextfocus == 'creditpaid')
                nextfocus = $scope.Settlement.paymentinfo.has_credit ? 'approvedby' : ($scope.system_loansettlement ? 'loanpaid' : 'voucher_id');

            if (nextfocus == 'cardpaid' && $scope.system_salecollectcarddata && $scope.Settlement.paymentinfo.card_paid > 0) {

                if (typeof ($scope.Settlement.paymentinfo.card_no) == 'undefined' || $scope.Settlement.paymentinfo.card_no == null || $scope.Settlement.paymentinfo.card_no.length != 4) {
                    nextfocus = 'cardno';
                    $scope['setFocus' + nextfocus]();
                    return;
                }
            }


            if (nextfocus == 'cardpaid' && $scope.Settlement.balanceamount && $scope.Settlement.balanceamount > 0
                && $scope.Settlement.balanceamount <= $scope.GetNumber($scope.settlement_maxadjustment)
                && !$scope.settlement_disableadjustment) {
                $scope.Settlement.paymentinfo.adjustment_paid = $scope.Settlement.balanceamount;
                $scope.Settlement.paymentinfo.adjustment_enable = true;
                $scope.Settlement.paidamount += $scope.Settlement.balanceamount;
                $scope.Settlement.balanceamount = $scope.Settlement.receivable - $scope.Settlement.paidamount;
                $scope.autoCoding.code = '';
                $timeout(function () {
                    $scope.setFocusadjustment_paid();
                }, 50);
            }
            else if ($scope.Settlement.balanceamount && $scope.Settlement.balanceamount > 0 &&
                nextfocus == 'cardpaid' &&
                $scope.UserInfo.appsettings.int_payment &&
                $scope.PaymentIntegration.Integrater &&
                $scope.PaymentIntegration.Integrater != null) {
                $scope['setFocus' + $scope.PaymentIntegration.Integrater.focusid]();
                return;
            }
            else if ($scope.Settlement.balanceamount >= 1) {
                $scope['setFocus' + nextfocus]();
            } else {
                $scope.Save(null, false);
                $scope.autoCoding.code = '';
                if (!$scope.IsAutoSettlement)
                    $scope.setFocusautocoding();
            }
        };

        $scope.codeChanged = function () {

            if (typeof ($scope.autoCoding.code) == 'undefined' || $scope.autoCoding.code == '') {
                $scope.CashChanged('0');
                return;
            };

            if ($scope.Settlement.Billsettlement == 'undefined' || $scope.Settlement.Billsettlement == null ||
                $scope.Settlement.Billsettlement.length == 0) {
                $scope.CashChanged('0');
                return;
            }
            //if ($scope.billprefix != null) 
            if (!$scope.BillNoConfig.isbillno($scope.autoCoding.code)) {
                //if ($scope.autoCoding.code.length < $scope.billprefix.length) {
                //    if (!$scope.billprefix.startsWith($scope.autoCoding.code)) {
                //        if ($scope.CashChanged($scope.autoCoding.code)) {
                //            $scope.Settlement.paymentinfo.cash_dinamination = $scope.autoCoding.code;
                //        }
                //        else {
                //            $scope.Settlement.paymentinfo.cash_dinamination = '';
                //        }
                //    }
                //}
                //else if (!$scope.autoCoding.code.startsWith($scope.billprefix)) {
                if ($scope.CashChanged($scope.autoCoding.code)) {
                    $scope.Settlement.paymentinfo.cash_dinamination = $scope.autoCoding.code;
                }
                else {
                    $scope.Settlement.paymentinfo.cash_dinamination = '';
                }
                //}
            }
            else if ($scope.CashChanged($scope.autoCoding.code)) {
                $scope.Settlement.paymentinfo.cash_dinamination = $scope.autoCoding.code;
            }
            else {
                $scope.Settlement.paymentinfo.cash_dinamination = '';
            }
        }

        $scope.SearchCustomer = function () {
            var request = {
                "Item": $scope.LoayaltyCutomer
            }

            $http({
                method: 'POST',
                url: "CustomerService/Search",
                data: JSON.stringify(request)
            }).success(function (result) {
                $scope.LoayaltyCutomer = result[0];

            }).error(function (result) {
                $scope.ErrorMessage = result;
            });
        }


        $timeout(function () {
            if ($scope.IsAutoSettlement) {
                $scope.setFocuscashpaid();
            }
            else {
                $scope.setFocusautocoding();
                $scope.SearchUnsettled(true);
                $scope.SearchLastSettlement();
            }
        }, 10);

        $scope.CreateNew();
        $scope.PaymentIntegrationStatus = '';
        $scope.PaymentIntegrationRunning = false;
        $scope.PaymentIntegrationCallBack = function (status, response,provider) {
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
                else if (response.PaymentMode == "EMI")
                {
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
                $scope.Save(null, false);
            }

            if (status == 4) {
                $scope.PaymentIntegrationStatus = 'Completed';
                $scope.PaymentIntegrationRunning = false;
                $scope.Settlement.intvendor = 1;
                $scope.Settlement.intmode = optionid;
                $scope.Settlement.paymentinfo.deviceresult = response;
                $scope.Settlement.paymentinfo.card_paid = $scope.Settlement.balanceamount;
                $scope.Settlement.paymentinfo.card_no = result.Card_Num;
                var bank = $scope.BankList.find(function (item) {
                    return item.code == response.Acquirer_Bank || item.name == response.Acquirer_Bank;
                });
                $scope.Settlement.paymentinfo.card_bankid = bank && bank != null ? bank.id : 0;
                var terminal = $scope.SwippingList.find(function (item) {
                    return item.code == result.Terminal_Id || item.name == response.Terminal_Id;
                });
                $scope.Settlement.paymentinfo.card_machineid = terminal && terminal != null ? terminal.id : 0;
                $scope.PaidChanged('CARD');
                $scope.Save(null, false);
            }
        }

        if ($scope.IsAutoSettlement) {
            var bill = $scope.ngDialogData[1];
            if (bill == null) return;

            $scope.Settlement.billmastercode = bill.billmasterid;
            $scope.Settlement.customername = bill.customername;
            $scope.Settlement.customermobileno = bill.phoneno;
            $scope.Settlement.customerid = bill.customerid;

            var totalpayable = bill.total;
            if (bill.additionalcharges &&
                bill.additionalcharges != null &&
                bill.additionalcharges > 0) {
                totalpayable += (bill.additionalcharges).round(2);
            }

            $scope.Settlement.tax = bill.tax;
            $scope.Settlement.discount = bill.discount;
            $scope.Settlement.billadditionaldiscount = bill.additionaldiscount;
            $scope.Settlement.billamount = totalpayable;
            $scope.Settlement.receivable = totalpayable;
            $scope.Settlement.billdate = bill.billdate;

            if ($scope.Settlement.receivable < 0)
                $scope.Settlement.returnbillvalue = Math.abs(totalpayable);

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

            if (sbill.iscreditbill) {
                $scope.Settlement.paymentinfo.credit_paid = $scope.GetNumber(bill.total);
                $scope.Settlement.paymentinfo.has_credit = true;
            }

            if (typeof ($scope.Settlement.additionaldiscount) == 'undefined' || isNaN($scope.Settlement.additionaldiscount) || $scope.Settlement.additionaldiscount == null)
                $scope.Settlement.additionaldiscount = 0;
            if (typeof ($scope.Settlement.billadditionaldiscount) == 'undefined' || isNaN($scope.Settlement.billadditionaldiscount) || $scope.Settlement.billadditionaldiscount == null)
                $scope.Settlement.billadditionaldiscount = 0;
            if (typeof (bill.emidetail) != 'undefined' && bill.emidetail != null) {
                $scope.Settlement.paymentinfo.loan_paid = bill.emidetail.loanamount - bill.emidetail.emicharges;
            }
            $timeout(function () {
                $scope.CashChanged('0');
            }, 10);
        }
        $scope.openNav = function () {
            document.getElementById("mySidenav").style.width = "50px";
        }
        $scope.closeNav = function () {
            document.getElementById("mySidenav").style.width = "0";
        }

        $scope.SearchCondition = {
            locationid: 0,
            fromdate: new Date(),
            todate: new Date()
        };

        $scope.SearchUnsettled = function (schedule) {
            $scope.unsettlebillcount = restService.Get('BillmasterService/UnsettledBillCount/' + $rootScope.LocationInfo.location.id + '/' + $rootScope.LocationInfo.floorid, true).then(function (results) {
                $scope.unsettlebillcount = results;
                if (schedule) {
                    $timeout(function () {
                        $scope.SearchUnsettled();
                    }, 5000);
                }
            });
        }

        $scope.SearchLastSettlement = function () {
            restService.Get('BillsettlementmasterService/SearchLastSettlements').then(function (results) {
                if (results != null && results.length == 1) {
                    $scope.lastMasterBillID = results[0].id;
                    $scope.lastbillno = encodeURIComponent(results[0].billno);
                }
            });
        }

        $scope.$on("F2Pressed", function () {
            $scope.setFocusautocoding();
        });
    }]);