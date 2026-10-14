
var app = angular.module('InfinityLab');

app.controller('CouponSchemeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'Excel', '$stateParams', 'localStorageService', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, Excel, $stateParams, localStorageService, $filter) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;
        $scope.IsCouponReport = $stateParams.type == 'CouponReport';                // This is the Coupon Consumption report
        $scope.IsCouponIssueReport = $stateParams.type == 'CouponIssueReport';      // 

        $scope.Type = [{ 'id': 1, 'name': 'Gift Vouchar' }, { 'id': 2, 'name': 'Gift Card' }];
        $scope.CouponModes = [{ 'id': 1, 'name': 'Unlimitted Discount Coupons' }, { 'id': 2, 'name': 'Unique Discount Coupons' }];
        $scope.CouponList = null;

        $scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            results.push({ 'id': 0, 'code': 'All Locations' });
            $scope.Locations = results;
        });

        $scope.CouponList = restService.Get('ReferencelistService/COUPONS', true).then(function (results) {
            results.push({ 'id': -8, 'name': 'SETTLEMENT SCHEME - SYSTEM' });
            results.push({ 'id': -9, 'name': 'BIRTHDAY COUPON - SYSTEM' });
            $scope.CouponList = results;
        });

        $scope.VoucherTypeList = [{ 'id': 0, 'name': 'ALL' },
        { 'id': 1, 'name': 'GIFT VOUCHER' },
        { 'id': 7, 'name': 'CREDIT NOTE' },
        { 'id': 8, 'name': 'CUSTOMER ADVANCE' }];

        $scope.CouponReportList = [{ 'id': 0, 'name': 'ISSUED' },
        { 'id': 1, 'name': 'RECEIVED' },
        { 'id': 2, 'name': 'PENDING' }];

        $scope.CouponEntity = {
            'name': '', 'validfrom': null, 'mode': 0, 'validto': null, 'issuedby': 0, 'amount': 0, 'amountaspercentage': false, 'rules': []
        };
        $scope.Coupon = (JSON.parse(JSON.stringify($scope.CouponEntity)));
        $scope.CouponRuleEntity = {
            'mode': 0, 'acceptablelocationid': 0, 'minbillvalue': 0, 'amountaspercentage': false, 'isranged': false,
            'maxbilldiscountpercentage': 0, 'rangedrule': []
        };
        $scope.CouponRule = (JSON.parse(JSON.stringify($scope.CouponRuleEntity)));

        $scope.Entity = {
            id: "",
            createdby: "",
            modifiedby: "",
            createdon: "",
            modifiedon: "",
            isactive: true,
            version: "",
            type: "",
            name: "",
            mobileno: "",
            validfrom: "",
            validto: "",
            balance: "",
            status: "",
            customerid: "",
            receivedamt: "",
            vouchervalue: "",
            issuedby: "",
            denomination: [{ denomination: 2000, qty: 0, amount: 0 },
            { denomination: 1000, qty: 0, amount: 0 },
            { denomination: 500, qty: 0, amount: 0 },
            { denomination: 200, qty: 0, amount: 0 },
            { denomination: 100, qty: 0, amount: 0 },
            { denomination: 50, qty: 0, amount: 0 }]
        };

        $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.ConsumptionResult = [];
        $scope.CouponIssueResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            customerid: ""
        }

        $scope.RangeRuleItem = { 'fromamount': 0, 'toamount': 0, 'amountaspercentage': false, 'amount': 0 };
        $scope.AddRangedRules = function () {
            if (typeof ($scope.CouponRule.rangedrule) == 'undefined' || $scope.CouponRule.rangedrule == null)
                $scope.CouponRule.rangedrule = [];
            $scope.CouponRule.rangedrule.push($scope.RangeRuleItem);
            $scope.RangeRuleItem = { 'fromamount': 0, 'toamount': 0, 'amountaspercentage': false, 'amount': 0 };
        }
        $scope.RemoveRangedRules = function (item) {
            $scope.CouponRule.rangedrule.splice($scope.CouponRule.rangedrule.indexOf(item), 1);
        }

        $scope.AmountChanged = function (item) {
            var oldValue = item.amount;
            item.amount = isNaN(item.denomination * item.qty) ? 0 : item.denomination * item.qty;
            $scope.GiftVoucher.vouchervalue = $scope.GiftVoucher.vouchervalue - oldValue + item.amount;
        };

        $scope.SaveCoupon = function () {

            $scope.Coupon.counterid = $rootScope.LocationInfo.counter.id;
            $scope.Coupon.locationid = $rootScope.LocationInfo.location.id;

            if ($scope.Coupon.locationid == 'undefined' || $scope.Coupon.locationid == null ||
                $scope.Coupon.locationid == 0) {
                $rootScope.Toaster.Error("Please Select Location");
                return;
            }


            if ($scope.Coupon.counterid == 'undefined' || $scope.Coupon.counterid == null ||
                $scope.Coupon.counterid == 0) {
                $rootScope.Toaster.Error("Please Select Counter");
                return;
            }

            request = {
                "Item": $scope.Coupon,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SaveCoupon",
                data: srequest
            }).success(function (result) {

                if (result != null) {
                    if (result.errormessage != null && result.errormessage.length > 0) {
                        $rootScope.Toaster.Error(result.errormessage);
                    }
                    else {
                        $rootScope.Toaster.Info("Coupon Saved Successfully.!");
                        $scope.Coupon = result;
                    }
                }

            }).error(function (result) {
            });
        }

        $scope.GenerateCoupon = function () {
            $scope.PrinterData = localStorageService.get('PrinterData');
            if ($scope.PrinterData == undefined ||
                $scope.PrinterData == null) {
                $rootScope.Toaster.Error('Please select the printer');
            }
            if ($scope.CustomerReward &&
                $scope.CustomerReward != null &&
                $scope.CustomerReward.customerid &&
                $scope.CustomerReward.customerid > 0) {

                $scope.CustomerReward.claimamount = $scope.Reward.claimamount;
                $scope.CustomerReward.claimpoint = $scope.Reward.claimpoint;
                $scope.CustomerReward.id = $scope.CustomerReward.customerid;
                $scope.CustomerReward.otp = $scope.Reward.pinno;
                $scope.CustomerReward.locationid = $scope.Reward.locationid; 
                if ($scope.Reward.mode == 'Multiple') {
                    $scope.CustomerReward.couponlist = $scope.Reward.couponlist;
                }
                request = {
                    "Item": $scope.CustomerReward,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "CustomerrewardsService/GenerateCoupon",
                    data: srequest
                }).success(function (result) {
                    $scope.PrinterData = localStorageService.get('PrinterData');
                    $rootScope.Sent2Printer(result, $scope.PrinterData, 'RAW', 'BASE64');
                });
            }
        }

        $scope.ShowPIN = false;
        $scope.SentCustomerOTP = function () {
            if ($scope.CustomerReward &&
                $scope.CustomerReward != null &&
                $scope.CustomerReward.customerid &&
                $scope.CustomerReward.customerid > 0) {

                var claimpoint = parseFloat($scope.Reward.claimpoint);
                var claimamount = parseFloat($scope.Reward.claimamount);

                if (isNaN(claimamount) || isNaN(claimpoint)) {
                    $rootScope.Toaster.Info("Please enter the coupon amount and adjustable point");
                    return;
                }

                if ($scope.CustomerReward.convertable < claimamount) {
                    $rootScope.Toaster.Info("Maximum allowed amount " + $scope.CustomerReward.convertable);
                    return;
                }

                if ($scope.CustomerReward.rewards < claimpoint) {
                    $rootScope.Toaster.Info("Maximum allowed point " + $scope.CustomerReward.rewards);
                    return;
                }

                $scope.CustomerReward.claimamount = $scope.Reward.claimamount;
                $scope.CustomerReward.claimpoint = $scope.Reward.claimpoint;
                $scope.CustomerReward.id = $scope.Reward.customerid;

                var request = {
                    "Item": {
                        'id': $scope.CustomerReward.customerid,
                        'claimpoint': $scope.Reward.claimpoint,
                        'claimamount': $scope.Reward.claimamount
                    },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "CustomerService/SendOTP",
                    data: srequest
                }).success(function (result) {
                    if (result != null) {
                        $rootScope.Toaster.Info(result);
                        $scope.ShowPIN = true;
                    }
                }).error(function (result) {
                });

            }
        }

        $scope.SearchCustomerReward = function (mode) {

            if ($scope.Reward == 'undefined' || $scope.Reward == null)
                return;

            $scope.ShowPIN = false;
            request = {
                "Item": { 'mobileno': '', 'loyeltyno': '', 'locationid': $scope.Reward.locationid },
                "securitytoken": "String content"
            }
            if (mode == 'loyeltyno') {
                if ($scope.Reward.loyeltyno == undefined ||
                    $scope.Reward.loyeltyno == null ||
                    $scope.Reward.loyeltyno == '') {
                    $rootScope.Toaster.Error('Please enter loyelty no');
                }
                request.Item.loyeltyno = $scope.Reward.loyeltyno;
            }
            else {
                if ($scope.Reward.mobileno == undefined ||
                    $scope.Reward.mobileno == null ||
                    $scope.Reward.mobileno == '') {
                    $rootScope.Toaster.Error('Please enter mobile no');
                }
                request.Item.mobileno = $scope.Reward.mobileno;
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CustomerrewardsService/SearchCustomerReward",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.CustomerReward = result;
                }
            }).error(function (result) {
            });
        }

        $scope.CurrentTab = 'Scheme';
        $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            $timeout(function () {
                $scope.ShowPIN = false;
                $scope.CurrentTab = $(e.target).text();
            }, 10);
        });

        $scope.CustomerReward = null;

        $scope.SearchCode = function () {
            if ($scope.GiftVoucher.name == 'C' || $scope.GiftVoucher.name == 'c') {
                $scope.ShowCustomerDialog();
                return;
            }
        };

        $scope.Addrule = function () {
            if ($scope.CouponRule.amountaspercentage) {
                if ($scope.CouponRule.amount.indexOf('%') >= 0) {
                    $scope.CouponRule.amount = $scope.CouponRule.amount.replace('%', '');
                }
            }
            $scope.CouponRule.printqty = 1; //$scope.CouponRule.quantity;

            if ($scope.CouponRule.rangedrule != 'undefined' && $scope.CouponRule.rangedrule != null
                && $scope.CouponRule.rangedrule.length == 1) {
                $rootScope.Toaster.Error("Please Use Single Rule option.");
                return;
            }
            if ($scope.CouponRule.rangedrule != 'undefined' && $scope.CouponRule.rangedrule != null
                && $scope.CouponRule.rangedrule.length > 1)
                $scope.CouponRule.isranged = true;

            if (!$scope.CouponRule.isranged) {
                if ($rootScope.GetNumber($scope.CouponRule.amount) == 0) {
                    $rootScope.Toaster.Error("Please enter discount amount/percentage");
                    return;
                }
            }
            $scope.Coupon.rules.push($scope.CouponRule);
            $scope.CouponRule = (JSON.parse(JSON.stringify($scope.CouponRuleEntity)));
        }

        $scope.DeleteRule = function (item) {
            $scope.Coupon.rules.splice($scope.Coupon.rules.indexOf(item), 1);
        }
        $scope.ViewRule = function (item) {
            $scope.CouponRule = item;
        }

        $scope.Delete = function () {
            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete the selected coupon scheme ?',
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
                                "Item": { 'id': $scope.Coupon.id, 'version': $scope.Coupon.version },
                                "SecurityToken": "String content"
                            }
                            // var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "CouponService/Delete",
                                data: request
                            }).success(function (result) {

                                if (result) {
                                    if (result.errormessage == "") {
                                        $rootScope.Toaster.Error("Successfully deleted");
                                        $scope.Coupon = (JSON.parse(JSON.stringify($scope.CouponEntity)));
                                    }
                                    else
                                        $rootScope.Toaster.Error(result.errormessage);
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

        $scope.ShowCouponList = false;
        $scope.SearchCouponList = function () {
            $scope.ShowCouponList = true;
            $scope.CouponList = [];
            $scope.CouponList = restService.Get('CouponService/SearchCouponList', true).then(function (results) {
                results.push({ 'id': -9, 'name': 'BIRTHDAY COUPON - SYSTEM' });
                $scope.CouponList = results;
            });
        }

        $scope.SearchSettlementCoupon = function (code) {
            request = {
                "Item": {
                    'code': code
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SearchSettlementCoupon",
                data: srequest
            }).success(function (result) {
                $scope.SettlementCoupon.customername = '';
                $scope.SettlementCoupon.mobileno = '';
                $scope.SettlementCoupon.customeraddress = '';
                $scope.SettlementCoupon.voucherids = [];
                $scope.SettlementCoupon.settlementmasterid = 0;
                if (result == null) return;
                $scope.SettlementCoupon.customername = result.customername;
                $scope.SettlementCoupon.mobileno = result.mobileno;
                $scope.SettlementCoupon.customeraddress = result.customeraddress;
                $scope.SettlementCoupon.voucherids = result.voucherids;
                $scope.SettlementCoupon.vouchervalue = result.vouchervalue;
                $scope.SettlementCoupon.validto = result.validto;
                $scope.SettlementCoupon.name = result.name;
                $scope.SettlementCoupon.settlementmasterid = result.settlementmasterid;
            });
        }

        $scope.SearchCustomer = function () {
            if ($scope.CouponRule.customermobileno == null || $scope.CouponRule.customermobileno == "") {
                $rootScope.Toaster.Info("Please enter the mobile no");
                return;
            }
            restService.Get('CustomerService/PhoneNumber?id=' + $scope.CouponRule.customermobileno, true).then(function (results) {
                $scope.CouponRule.customername = results.name;
                $scope.CouponRule.otpcustomerid = results.id;
            });
        }

        $scope.SelectAllChanged = function () {
            $.each($scope.SearchResult, function (k, v) {
                v.selected = $scope.SearchCondition.SelectAll;
            });
        }
        $scope.SearchCouponConsumption = function () {
            request = {
                "Item": {
                    'id': $scope.Coupon.id, 'code': $scope.Coupon.code,
                    'billfromdate': $scope.Coupon.billfromdate, 'billtodate': $scope.Coupon.billtodate
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SearchCouponConsumption",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.SearchResult = result;
                    $scope.couponcount = 0;
                    $scope.couponconsumed = 0;
                    $scope.discountvalue = 0;
                    $scope.couponcount = $scope.SearchResult.length;
                    $.each($scope.SearchResult, function (k, v) {
                        if (v.billno != '')
                            $scope.couponconsumed++;
                        if ($scope.GetNumber(v.discount) > 0)
                            $scope.discountvalue += $scope.GetNumber(v.discount);
                    });

                }
                else {
                    //$scope.Coupon = (JSON.parse(JSON.stringify($scope.CouponEntity)));
                    $scope.SearchResult = [];
                }

                if ($scope.IsCouponReport)
                    $scope.CouponReportConfig();

            }).error(function (result) {
            });

        }

        $scope.CouponReportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddSimpleColumn('name', 'Scheme Name', 180);
            igrid.AddSimpleColumn('issuelocation', 'Issue Location', 150);
            igrid.AddSimpleColumn('issuecounter', 'Issue Counter', 120);
            igrid.AddSimpleColumn('vouchercode', 'Code', 150);
            igrid.AddFormatedColumn('vouchervalue', 'Voucher Value', 100, 'N2', 2).total = true;
            igrid.AddCustomColumn('ispercentage', 'Voucher Value %', 60, function (item, col) {
                return item.ispercentage == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('issuetocustomername', 'Issue to', 100);
            igrid.AddSimpleColumn('issuetomobileno', 'Issue to Mobile', 80);
            igrid.AddFormatedColumn('validfrom', 'Valid From', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validto', 'Valid To', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('billvalue', 'Bill Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 100);
            igrid.AddSimpleColumn('customername', 'Customer Name', 150);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.LoadCouponHistory = function () {
            if ($scope.Coupon.searchid == -9) {
                $scope.SearchHistory('id', -9);
                return;
            }

            request = {
                "Item": { 'id': $scope.Coupon.searchid },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SearchCoupons",
                data: srequest
            }).success(function (result) {
                $scope.CouponRule = (JSON.parse(JSON.stringify($scope.CouponRuleEntity)));
                if (result.length > 0) {
                    $scope.Coupon = result[0];
                    $.each($scope.Coupon.rules, function (k, v) {
                        v.printqty = 1; //v.quantity;
                    });
                    $scope.ShowCouponList = false;
                    $scope.SearchResult = [];
                    $scope.SearchHistory('id', $scope.Coupon.id);
                }
                else {
                    $scope.Coupon = $scope.Coupon = (JSON.parse(JSON.stringify($scope.CouponEntity)));
                    $scope.SearchResult = [];
                }
            }).error(function (result) {
            });
        }
        $scope.LoadCoupon = function () {
            request = {
                "Item": { 'id': $scope.Coupon.searchid },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SearchCoupons",
                data: srequest
            }).success(function (result) {
                $scope.Coupon = result[0];
                $.each($scope.Coupon.rules, function (k, v) {
                    v.printqty = 1; //v.quantity;
                });
                $scope.ShowCouponList = false;
            }).error(function (result) {
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

        $scope.SearchHistory = function (mode, id) {

            if (mode == 'id') {
                $('.nav-tabs a[data-target="#history"]').tab('show')
            }

            $scope.Locations = restService.Get('CouponService/CouponHistory/' + encodeURIComponent(mode) + '/' + encodeURIComponent(id), true).then(function (results) {
                $scope.SearchResult = results;
                $scope.couponcount = 0;
                $scope.couponconsumed = 0;
                $scope.discountvalue = 0;
                $scope.couponcount = $scope.SearchResult.length;
                $.each($scope.SearchResult, function (k, v) {
                    if (v.billno != '')
                        $scope.couponconsumed++;
                    if ($scope.GetNumber(v.discount) > 0)
                        $scope.discountvalue += $scope.GetNumber(v.discount);
                });

            });
        }

        $scope.SearchVoucherList = function () {
            request = {
                "Item": { 'customerid': $scope.GiftVoucher.customerid },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CouponService/SearchVouchersList",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.PopupList = [];
                    $scope.Customer = null;
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
                        var request = {
                            "Item": { 'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno, 'hasgst': $scope.hasgst },
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
                    $scope.Search = function () {
                        var request = {
                            "Item": { 'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno, 'hasgst': $scope.hasgst },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            $scope.PopupList = result;
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document' && data.value.id > 0) {
                        $scope.CouponRule.customername = data.value.name;
                        $scope.CouponRule.otpcustomerid = results.id;
                    }
                }
            });
        }

        $scope.PrintCouponReceipt = function () {
            var list = [];
            $.each($scope.Coupon.rules, function (k, v) {
                if (v.selected) {
                    if (v.couponids != null && v.couponids.length > 0) {
                        $.each(v.couponids, function (kk, vv) {
                            list.push(vv);
                        })
                    }
                }
            });

            if (list.length == 0) {
                $rootScope.ShowAlert('Coupon', 'Please select the rules');
                return;
            }
            if ($rootScope.GetNumber($scope.Coupon.rules[0].printqty) == 0) {
                $rootScope.ShowAlert('Coupon', 'Please enter no of copy to print');
                return;
            }

            var requestitem = { 'items': list, 'couponid': $scope.Coupon.id, 'copy': $scope.Coupon.rules[0].printqty };
            $rootScope.Handlers.SelectPrinter('GIFTCOUPON', function (printdata) {
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
                    url: "CouponService/PrintCouponReceipt",
                    data: srequest
                }).success(function (result) {
                    if (result != null && result.Item != null && result.Item.printdocument)
                        $rootScope.Sent2Printer(result.Item.printdocument);
                });
            });
        }

        $scope.PrintSettlementCoupon = function () {
            var requestitem = {
                'items': $scope.SettlementCoupon.voucherids,
                'billsettlementmasterid': $scope.SettlementCoupon.settlementmasterid,
                'copy': 1
            };
            $rootScope.Handlers.SelectPrinter('COUPONPRINT', function (printdata) {
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
                    url: "CouponService/PrintCoupon",
                    data: srequest
                }).success(function (result) {
                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);

                        $scope.SettlementCoupon.Code = '';
                        $scope.SettlementCoupon.customername = '';
                        $scope.SettlementCoupon.mobileno = '';
                        $scope.SettlementCoupon.customeraddress = '';
                        $scope.SettlementCoupon.voucherids = [];
                        $scope.SettlementCoupon.settlementmasterid = 0;
                        $scope.SettlementCoupon.vouchervalue = 0;
                        $scope.SettlementCoupon.validto = null;
                        $scope.SettlementCoupon.name = '';
                    }
                });
            });
        }

        $scope.PrintCoupon = function () {

            var list = [];
            $.each($scope.Coupon.rules, function (k, v) {
                if (v.selected) {
                    if (v.couponids != null && v.couponids.length > 0) {
                        $.each(v.couponids, function (kk, vv) {
                            list.push(vv);
                        })
                    }
                }
            });

            if (list.length == 0) {
                $rootScope.ShowAlert('Coupon', 'Please select the rules');
                return;
            }
            if ($rootScope.GetNumber($scope.Coupon.rules[0].printqty) == 0) {
                $rootScope.ShowAlert('Coupon', 'Please enter no of copy to print');
                return;
            }

            var requestitem = { 'items': list, 'couponid': $scope.Coupon.id, 'copy': $scope.Coupon.rules[0].printqty };
            $rootScope.Handlers.SelectPrinter('COUPONPRINT', function (printdata) {
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
                    url: "CouponService/PrintCoupon",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.PrintBarcode = function () {
            var list = [];
            $.each($scope.SearchResult, function (k, v) {
                if (v.selected)
                    list.push({ 'id': v.id, 'code': v.code });
            });
            if (list.length == 0) {
                $rootScope.ShowAlert('Voucher barcode', 'Please select the Card Number');
                return;
            }
            $rootScope.Prompt('Voucher Barcode', 'No Of Copies', function (count) {
                var requestitem = { 'copy': count, 'items': list };
                $scope.ChoseReport(requestitem);
            });
        };


        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('COUPONPRINT', function (printdata) {
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
                    url: "CouponService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Export2Excel = function () {

            var list = [];
            $.each($scope.Coupon.rules, function (k, v) {
                if (v.selected) {
                    if (v.couponids != null && v.couponids.length > 0) {
                        $.each(v.couponids, function (kk, vv) {
                            list.push(vv);
                        })
                    }
                }
            });

            if (list.length == 0) {
                $rootScope.ShowAlert('Coupon barcode', 'Please select the rules');
                return;
            }

            var request = {
                "Item": { 'ids': list },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "VouchersService/Search",
                data: srequest
            }).success(function (result) {

                if (result != null && result.length > 0) {

                    var rows = [];
                    rows.push('<Thead>')
                    rows.push('<Tr><th>Code</th><th>Discount</th><tr>')
                    rows.push('</Tr></Thead><Tbody>')

                    var singlerow = [];
                    for (var i = 0; i < result.length; i++) {
                        singlerow.push('<td>');
                        singlerow.push(result[i].code);
                        singlerow.push('</td>');
                        singlerow.push('<td>');
                        singlerow.push(result[i].amount);
                        singlerow.push('</td>');
                        rows.push('<tr>');
                        rows.push(singlerow.join(''));
                        rows.push('</tr>');
                        singlerow = [];
                    }

                    rows.push('</Tbody>')

                    tableContent = rows.join('');

                    var uri = 'data:application/vnd.ms-excel;base64,',
                        template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table border="1">{table}</table></body></html>',
                        base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))); },
                        format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) };
                    var ctx = { worksheet: 'Coupons', table: tableContent };


                    var blob = new Blob([format(template, ctx)], { type: 'application/vnd.ms-excel;charset=utf-8;' });
                    if (navigator.msSaveBlob) { // IE 10+
                        navigator.msSaveBlob(blob, "Coupons.xls");
                        return null;
                    } else {
                        var link = document.createElement("a");
                        if (link.download !== undefined) { // feature detection
                            // Browsers that support HTML5 download attribute
                            var url = URL.createObjectURL(blob);
                            link.setAttribute("href", url);
                            link.setAttribute("download", "Coupons.xls");
                            link.style.visibility = 'hidden';
                            document.body.appendChild(link);
                            link.click();
                            document.body.removeChild(link);
                            return null;
                        }
                    }

                }
            });

        }

        $scope.Export = function () {
            if (typeof ($scope.igrid) == 'undefined') return;
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Gift Coupon Details');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 0, 'Coupon Transaction History');
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.exportGridtoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'COUPON CONSUMPTION REPORT';

            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.SearchVoucherConsumption = function () {
            request = {
                "Item": {
                    'code': $scope.Coupon.code, 'validfrom': $scope.Coupon.validfrom, 'validto': $scope.Coupon.validto,
                    'billfromdate': $scope.Coupon.billfromdate, 'billtodate': $scope.Coupon.billtodate,
                    'vouchertypeid': $scope.Coupon.vouchertypeid
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SearchVoucherConsumption",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.ConsumptionResult = result;
                }
                else {
                    $scope.ConsumptionResult = [];
                    $rootScope.Toaster.Info("No Results found.");
                }
                $scope.VoucherReportConfig();

            }).error(function (result) {
            });
        }

        $scope.CalculateRewardList = function (item) {
            var rewardsperrupee = ($scope.CustomerReward.convertable/$scope.CustomerReward.rewards).round(2);
            var claimamount = $scope.GetNumber(item.claimamount);
            item.claimpoint = (item.claimamount / rewardsperrupee).round(0);
            $scope.Reward.claimamount = 0;
            $scope.Reward.claimpoint = 0;
            $.each($scope.Reward.couponlist, function (k, v) {
                $scope.Reward.claimamount += $scope.GetNumber(v.claimamount);
                $scope.Reward.claimpoint += $scope.GetNumber(v.claimpoint);
            })
        }

        $scope.RefreshMultipleCoupons = function () {
            var noofcoupons = parseInt($scope.Reward.noofcoupons);
            if (isNaN(noofcoupons))
                noofcoupons = 1;
            $scope.Reward.couponlist = [];
            for (var i = 0; i < noofcoupons; i++) {
                $scope.Reward.couponlist.push({ 'claimamount': 0,'claimpoint':0});
            }
        }

        $scope.VoucherReportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.ConsumptionResult);

            igrid.AddSimpleColumn('vouchertypename', 'Voucher Type', 100);
            igrid.AddFormatedColumn('issuedon', 'Issued On', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('issuelocationname', 'Issue Location', 80);
            igrid.AddSimpleColumn('issuecountername', 'Issue Counter', 80);
            igrid.AddSimpleColumn('issuetono', 'Issue To No', 100);
            igrid.AddSimpleColumn('vouchercode', 'Code', 140);
            igrid.AddFormatedColumn('validfrom', 'Valid from', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validto', 'Valid upto', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('vouchervalue', 'Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('claim', 'Claimed', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('settlementcode', 'Settlement Code', 120);
            igrid.AddFormatedColumn('settlementon', 'Settlement On', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('location', 'Settlement Location', 100);
            igrid.AddSimpleColumn('settlementcountername', 'Settlement Counter', 100);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 80);
            igrid.AddSimpleColumn('name', 'Customer Name', 120);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.ConsumptionResult);
        }

        $scope.SearchCondition = {
            locationid: 0,
            fromdate: null,
            todate: null,
        }

        $scope.SearchCouponIssueReport = function () {
            request = {
                "Item": {
                    'validfrom': $scope.SearchCondition.fromdate, 'validto': $scope.SearchCondition.todate,
                    'locationid': $scope.SearchCondition.locationid, 'vouchertypeid': $scope.SearchCondition.vouchertypeid,
                    'iscoupononly': $scope.IsCouponIssueReport
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SearchCouponIssue",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.CouponIssueResult = result;
                }
                else {
                    $scope.CouponIssueResult = [];
                    $rootScope.Toaster.Info("No Results found.");
                }

                if ($scope.IsCouponIssueReport)
                    $scope.CouponIssueReportConfig();
                else
                    $scope.SettlementCouponIssueReportConfig();

            }).error(function (result) {
            });

        }

        $scope.CouponIssueReportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CouponIssueResult);

            igrid.AddSimpleColumn('issuelocation', 'Location', 100);
            igrid.AddSimpleColumn('countername', 'Counter', 100);
            igrid.AddSimpleColumn('customername', 'Customer', 100);
            igrid.AddSimpleColumn('vouchercode', 'Coupon Code', 140);
            igrid.AddSimpleColumn('schemename', 'Scheme Name', 100);
            igrid.AddFormatedColumn('vouchervalue', 'Coupon Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('issuedon', 'Issued On', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validfrom', 'Valid From', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validto', 'Valid Upto', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('billno', 'Settlement Code#', 100);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.CouponIssueResult);
        }

        $scope.SettlementCouponIssueReportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CouponIssueResult);

            igrid.AddSimpleColumn('customername', 'Location', 100);
            igrid.AddSimpleColumn('countername', 'Counter', 100);
            igrid.AddSimpleColumn('name', 'Coupon ID', 100);
            igrid.AddSimpleColumn('issuedbyname', 'Issued By', 120);
            igrid.AddSimpleColumn('vouchercode', 'Coupon Code', 140);
            igrid.AddFormatedColumn('vouchervalue', 'Coupon Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('issuedon', 'Issued On', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validfrom', 'Valid From', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validto', 'Valid Upto', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.CouponIssueResult);
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
                //}).success(function(result) {

                //}).error(function(error) {

                //});
            })
        }
    }]);