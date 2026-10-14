
var app = angular.module('InfinityLab');

app.controller('GiftVoucherController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'localStorageService', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, localStorageService, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.HasLocationSelected = false;
        $scope.HasCounterSelected = false;

        $scope.VoucherNewValiddate = new Date();
        $scope.VoucherId = 0;
        $scope.PaymentModes = [
            { 'id': 0, 'name': 'Free' },
            { 'id': 1, 'name': 'Transfer' },
            { 'id': 2, 'name': 'Cheque/DD' },
            { 'id': 3, 'name': 'Cash' },
            { 'id': 4, 'name': 'Card' },
            { 'id': 5, 'name': 'UPI' },
            { 'id': 6, 'name': 'Credit' },
            { 'id': 7, 'name': 'Expense' }];

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });

        $scope.Type = [{ 'id': 1, 'name': 'Gift Vouchar' }, { 'id': 2, 'name': 'Gift Card' }];

        $scope.VoucherIssueModes = [{ 'id': 0, 'name': 'Direct' }, { 'id': 1, 'name': 'Pre-Printed' },
        { 'id': 2, 'name': 'Issue Printed' }, { 'id': 3, 'name': 'Delete Issue' }];

        $scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            results.push({ 'id': 0, 'code': 'All Locations' });
            $scope.Locations = results;
        });

        $scope.UPIProviderList = restService.Get('ReferencelistService/UPIPROVIDER', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-' });
            }
            $scope.UPIProviderList = results;
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

        $scope.SwippingList = restService.Get('ReferencelistService/FULL-SWIPPINGMACINE', true).then(function (results) {
            $scope.SwippingList = results;
            $rootScope.OnLocationInfoReady(function (locationinfo) {
                var SwippingList = [];
                $.each(results, function (k, v) {
                    if (typeof (v.additionalinfo) != 'undefined' &&
                        typeof (v.additionalinfo.locationid) != 'undefined') {
                        if (v.additionalinfo.locationid == $rootScope.LocationInfo.location.id)
                            SwippingList.push(v);
                    }
                    else {
                        SwippingList.push(v);
                    }
                });
                $scope.SwippingList = SwippingList;
            })
        });

        $scope.CreateNew = function () {
            $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.GiftVoucher.counterid = $rootScope.LocationInfo.counter.id;
            $scope.GiftVoucher.locationid = $rootScope.LocationInfo.location.id;
            $scope.ShowCouponList = false;
            $scope.CustomerSearchResult = [];

            var tdate = new Date();
            tdate = tdate.setDate(tdate.getDate() + 1);
            $scope.GiftVoucher.validfrom = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.GiftVoucher.validto = $filter('date')(tdate, datePicker.shortDisplay);
        };


        $scope.SearchCouponNo = function (couponno) {
            var url = 'VouchersService/IssuedCoupon/' + encodeURIComponent(couponno);
            restService.Get(url, true).then(function (results) {
                if (results && results.length == 1) {

                    if ($scope.GiftVoucher.issuemode == 2) {
                        if (results[0].status != -1) {
                            $rootScope.Toaster.Error("This coupon already issued to " + results[0].customername + ' by ' +
                                results[0].employeename + ' on ' + results[0].issuedon);
                            return;
                        }
                        $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));
                        $scope.GiftVoucher.issuemode = 2;
                        $scope.GiftVoucher.discountpercentage = results[0].discountpercentage;
                        $scope.GiftVoucher.discountvalue = results[0].discountvalue;
                        $scope.GiftVoucher.vouchervalue = results[0].amount;
                        $scope.GiftVoucher.validfrom = results[0].issuedon;
                        $scope.GiftVoucher.validto = results[0].validupto;
                        $scope.GiftVoucher.voucherid = results[0].id;
                        $scope.GiftVoucher.counterid = $rootScope.LocationInfo.counter.id;
                        $scope.GiftVoucher.locationid = $rootScope.LocationInfo.location.id;
                        if (results[0].discountpercentage > 0) {
                            $scope.GiftVoucher.receivedamt = $scope.GiftVoucher.vouchervalue - (results[0].amount * results[0].discountpercentage / 100);
                        } else if (results[0].discountvalue > 0) {
                            $scope.GiftVoucher.receivedamt = $scope.GiftVoucher.vouchervalue - results[0].discountvalue;
                        }
                        $scope.GiftVoucher.receivableamt = $scope.GiftVoucher.receivedamt;
                    }
                    else if ($scope.GiftVoucher.issuemode == 3) {
                        if (results[0].status == -1) {
                            $rootScope.Toaster.Error("This coupon is not allocated to any customer");
                            return;
                        }
                        if (results[0].status != 0) {
                            $rootScope.Toaster.Error("This coupon already issued to " + results[0].customername + ' by ' +
                                results[0].employeename + ' on ' + results[0].issuedon + ' - Consumed');
                            return;
                        }
                        $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));
                        $scope.GiftVoucher.issuemode = 3;
                        $scope.GiftVoucher.issuedby = results[0].issuedby;
                        $scope.GiftVoucher.discountpercentage = results[0].discountpercentage;
                        $scope.GiftVoucher.discountvalue = results[0].discountvalue;
                        $scope.GiftVoucher.vouchervalue = results[0].amount;
                        $scope.GiftVoucher.validfrom = results[0].issuedon;
                        $scope.GiftVoucher.validto = results[0].validupto;
                        $scope.GiftVoucher.voucherid = results[0].id;
                        $scope.GiftVoucher.mobileno = results[0].mobileno;
                        $scope.GiftVoucher.givento = results[0].customername;

                        $scope.GiftVoucher.counterid = $rootScope.LocationInfo.counter.id;
                        $scope.GiftVoucher.locationid = $rootScope.LocationInfo.location.id;
                        if (results[0].discountpercentage > 0) {
                            $scope.GiftVoucher.receivedamt = $scope.GiftVoucher.vouchervalue - (results[0].amount * results[0].discountpercentage / 100);
                        } else if (results[0].discountvalue > 0) {
                            $scope.GiftVoucher.receivedamt = $scope.GiftVoucher.vouchervalue - results[0].discountvalue;
                        }
                    }

                }
            });
        }

        $scope.Counters = null;
        restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
        });

        $scope.Locations = null;
        restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.CanRevokeGiftVoucher = false;
        $scope.system_swippingmachine = false;
        $scope.Ledgers = null;

        $scope.FinanceLedgerChanged = function () {
            $scope.GiftVoucher.bankid = 0;
            var ledger = $scope.Ledgers.find(function (item) {
                return item.id == $scope.GiftVoucher.ledgerid;
            });
            if (ledger && ledger != null) {
                $scope.GiftVoucher.bankid = ledger.bankid;
            }
        }


        $scope.ShowLocationInfo = function () {
            if ($rootScope == undefined || $rootScope.UserInfo == undefined ||
                $rootScope.UserInfo.appsettings == undefined || $scope.Counters == null ||
                $scope.Locations == null || $scope.BuyingCompany == null) {
                $timeout(function () {
                    $scope.ShowLocationInfo();
                }, 50);
                return;
            }

            $scope.HasLocationSelected = $rootScope.LocationInfo && $rootScope.LocationInfo.location;
            $scope.HasCounterSelected = $rootScope.LocationInfo && $rootScope.LocationInfo.counter;
            $scope.FinanceEnabled = $rootScope.UserInfo.appsettings.int_finance;
            if ($scope.FinanceEnabled && $scope.Ledgers == null) {
                $scope.Ledgers = restService.Get('ReferencelistService/LEDGERS/6_11_10_5', true).then(function (results) {
                    $scope.Ledgers = results;
                });
            }

            if ($scope.HasCounterSelected) {
                $scope.GiftVoucher.counterid = $rootScope.LocationInfo.counter.id;
                $scope.CounterName = $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters);
            }
            else {
                $scope.CounterName = 'SELECT COUNTER';
            }

            if ($scope.HasLocationSelected) {
                $scope.GiftVoucher.locationid = $rootScope.LocationInfo.location.id;
                $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
            }
            else {
                $scope.LocationName = 'SELECT LOCATION';
            }

            $scope.CanRevokeGiftVoucher = UserDetails.Info.p_REVOKEGIFTVOUCHER && UserDetails.Info.p_REVOKEGIFTVOUCHER.view;
            $scope.system_swippingmachine = $rootScope.UserInfo.appsettings.system_swippingmachine;
        }

        $timeout(function () {
            $scope.ShowLocationInfo();
        }, 50);

        $scope.Entity = {
            id: "",
            createdby: "",
            modifiedby: "",
            createdon: "",
            modifiedon: "",
            isactive: true,
            issuemode: 0,
            mode: 0,
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
            denomination: [{ denomination: 25000, qty: 0, amount: 0 }, { denomination: 15000, qty: 0, amount: 0 },
            { denomination: 10000, qty: 0, amount: 0 }, { denomination: 5000, qty: 0, amount: 0 },
            { denomination: 2000, qty: 0, amount: 0 },
            { denomination: 1000, qty: 0, amount: 0 },
            { denomination: 500, qty: 0, amount: 0 },
            { denomination: 200, qty: 0, amount: 0 },
            { denomination: 100, qty: 0, amount: 0 },
            { denomination: 50, qty: 0, amount: 0 },
            { denomination: 0, qty: 0, amount: 0, 'iscustom': true }]
        };

        $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.BuyingCompany = null;
        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            customerid: ""
        }

        $scope.AmountChanged = function (item) {
            var oldValue = $rootScope.GetNumber(item.amount);
            if (item.denomination == -1) {
                item.amount = $rootScope.GetNumber(item.qty);
            }
            else {
                item.amount = isNaN(item.denomination * item.qty) ? 0 : $rootScope.GetNumber(item.denomination * item.qty);
            }
            $scope.GiftVoucher.vouchervalue = $rootScope.GetNumber($scope.GiftVoucher.vouchervalue) - $rootScope.GetNumber(oldValue) + $rootScope.GetNumber(item.amount);
        };

        $scope.Revoke = function () {

            if (!$scope.CanRevokeGiftVoucher) {
                $rootScope.Toaster.Info("You don't have permissin to revoke the issued voucher");
                return;
            }

            request = {
                "Item": $scope.GiftVoucher,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/RemoveIssue",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    if (result.errormessage != null && result.errormessage.length > 0) {
                        $rootScope.Toaster.Error(result.errormessage);
                        return;
                    }
                    $rootScope.Toaster.Info("Voucher Rewoked Successfully.!");
                }
            }).error(function (result) {
            });
        }

        $scope.Issue = function () {

            if ($scope.GiftVoucher.voucherid == undefined ||
                $scope.GiftVoucher.voucherid == null ||
                $scope.GiftVoucher.voucherid <= 0) {
                $rootScope.Toaster.Error("Please enter valid coupon code and search again.");
                return;
            }

            if ($scope.GiftVoucher.counterid == 'undefined' || $scope.GiftVoucher.counterid == null ||
                $scope.GiftVoucher.counterid == 0) {
                $rootScope.Toaster.Error("Please Select Location and Counter");
                return;
            }

            if ($scope.GiftVoucher.issuedby == 'undefined' || $scope.GiftVoucher.issuedby == null ||
                $scope.GiftVoucher.issuedby == 0) {
                $rootScope.Toaster.Error("Please Select Issued by Employee");
                return;
            }

            if ($scope.GiftVoucher.customerid == 'undefined' || $scope.GiftVoucher.customerid == null ||
                $scope.GiftVoucher.customerid == 0) {
                $rootScope.Toaster.Error("Please Select Issued to Customer");
                return;
            }

            if ($scope.GiftVoucher.receivableamt > $rootScope.GetNumber($scope.GiftVoucher.receivedamt)) {
                $rootScope.Toaster.Error("Receivable Amount - " + $scope.GiftVoucher.receivableamt);
                return;
            }


            request = {
                "Item": $scope.GiftVoucher,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/IssueVoucher",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    if (result.errormessage != null && result.errormessage.length > 0) {
                        $rootScope.Toaster.Error(result.errormessage);
                        return;
                    }
                    $rootScope.Toaster.Info("Voucher Issued Successfully.!");
                }
            }).error(function (result) {
            });
        }

        $scope.Save = function () {

            if ($scope.GiftVoucher.companyid == 'undefined' || $scope.GiftVoucher.companyid == null ||
                $scope.GiftVoucher.companyid == 0) {
                $rootScope.Toaster.Error("Please Select Company");
                return;
            }


            if ($scope.GiftVoucher.locationid == 'undefined' || $scope.GiftVoucher.locationid == null ||
                $scope.GiftVoucher.locationid == 0) {
                $rootScope.Toaster.Error("Please Select Location");
                return;
            }


            if ($scope.GiftVoucher.counterid == 'undefined' || $scope.GiftVoucher.counterid == null ||
                $scope.GiftVoucher.counterid == 0) {
                $rootScope.Toaster.Error("Please Select Counter");
                return;
            }

            if ($scope.GiftVoucher.issuedby == 'undefined' || $scope.GiftVoucher.issuedby == null ||
                $scope.GiftVoucher.issuedby == 0) {
                $rootScope.Toaster.Error("Please Select Issued by Employee");
                return;
            }

            request = {
                "Item": $scope.GiftVoucher,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SaveVoucher",
                data: srequest
            }).success(function (result) {

                if (result != null) {
                    if (result.errormessage != null && result.errormessage.length > 0) {
                        $rootScope.Toaster.Error(result.errormessage);
                    }
                    else if (result.id > 0) {
                        $rootScope.Toaster.Info("Voucher Saved Successfully.!");
                        $scope.GiftVoucher = result;
                    }
                }

            }).error(function (result) {
            });
        }

        $scope.SearchCode = function () {
            if ($scope.GiftVoucher.name == 'C' || $scope.GiftVoucher.name == 'c') {
                $scope.ShowCustomerDialog();
                return;
            }
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

        $scope.CashChanged = function (cash) {
            var value = 0;
            if (typeof (cash) == 'string' && cash.length > 0) {
                try {
                    value = eval(cash);

                    if (typeof (value) == 'undefined' || value == null || typeof (value) != 'number' || isNaN(value)) {
                        vaule = 0;
                        $scope.GiftVoucher.paymentinfo.cash_paid = '';
                        $scope.GiftVoucher.paymentinfo.cash_dinamination = '';
                        $scope.GiftVoucher.paymentinfo.cash_return = 0;
                    }
                } catch (exp) {
                }
            }
            cash = value;
            $scope.GiftVoucher.paymentinfo.cash_paid = $scope.GetNumber(cash) == 0 ? '' : cash;
            $scope.PaidChanged();
            return value != 0;
        }

        $scope.PaidChanged = function () {


            $scope.GiftVoucher.paidamount = $scope.GetNumber($scope.GiftVoucher.paymentinfo.cash_paid) +
                $scope.GetNumber($scope.GiftVoucher.paymentinfo.card_paid) +
                $scope.GetNumber($scope.GiftVoucher.paymentinfo.upi_paid);

            $scope.GiftVoucher.paidamount = $scope.GetNumber($scope.GiftVoucher.paidamount);
            $scope.GiftVoucher.balanceamount = $scope.GiftVoucher.receivedamt - $scope.GiftVoucher.paidamount;

            if (Math.abs($scope.GiftVoucher.balanceamount) < 1)
                $scope.GiftVoucher.balanceamount = 0;

            if ($scope.GiftVoucher.balanceamount < 0) {
                $scope.GiftVoucher.paymentinfo.cash_return = $scope.GiftVoucher.balanceamount * -1;
            }
            else {
                $scope.GiftVoucher.paymentinfo.cash_return = 0;
            }
        }


        $scope.ShowCouponList = false;
        $scope.CouponList = null;
        $scope.SearchCouponList = function () {
            $scope.ShowCouponList = true;
            if ($scope.CouponList == null) {
                $scope.CouponList = restService.Get('CouponService/SearchVoucherList', true).then(function (results) {
                    $scope.CouponList = results;
                });
            }
        }

        $scope.SearchCustomer = function () {
            if ($scope.GiftVoucher.mobileno == null || $scope.GiftVoucher.mobileno == "") {
                $scope.GiftVoucher.name = "Cash";
                return;
            }
            restService.Get('CustomerService/PhoneNumber?id=' + $scope.GiftVoucher.mobileno, true).then(function (results) {
                $scope.GiftVoucher.name = results.name;
                $scope.GiftVoucher.customerid = results.id;
                if (results.id == 'undefined' || results.id == null || results.id == "") {
                    $rootScope.Toaster.Error("Can't find Customer, please refine your search.");
                    return;
                }
                $scope.SearchVoucherList();
            });
        }

        $scope.SelectAllChanged = function () {
            $.each($scope.PrintCouponList, function (k, v) {
                v.selected = $scope.SearchCondition.SelectAll;
            });
        }

        $scope.LoadCoupon = function (id,switchhome) {
            if (typeof (id) == 'undefined') return;
            request = {
                "Item": { 'id': id },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/SearchVoucher",
                data: srequest
            }).success(function (result) {
                $scope.GiftVoucher = result[0];
                $scope.PrintCouponList = result[0].coupons;
                $scope.CustomerSearchResult = result;
                if (switchhome) {
                    $('.nav-tabs a[data-target="#giftvoucher"]').tab('show')
                }
                else {
                    $scope.ShowCouponList = false;
                }
                $scope.CounterName = $scope.SearchRef($scope.GiftVoucher.counterid, $scope.Counters);
                $scope.LocationName = $scope.SearchRef($scope.GiftVoucher.locationid, $scope.Locations);


            }).error(function (result) {
            });
        }

        $scope.SearchHistory = function (mode, id) {

            if (mode == 'id') {
                $('.nav-tabs a[data-target="#history"]').tab('show')
            }

            $scope.Locations = restService.Get('CouponService/VoucherHistory/' + encodeURIComponent(mode) + '/' + encodeURIComponent(id), true).then(function (results) {
                $scope.SearchResult = results;
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
                url: "CouponService/SearchVoucher",
                data: srequest
            }).success(function (result) {

                $scope.CustomerSearchResult = result;
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
                        $scope.GiftVoucher.mobileno = data.value.mobileno;
                        $scope.GiftVoucher.name = data.value.name;
                        $scope.GiftVoucher.name = results.name;
                        $scope.GiftVoucher.customerid = results.id;
                        $scope.SearchVoucherList();
                    }
                }
            });
        }

        $scope.PrintVoucherLabel = function () {
            var codes = '~'
            $.each($scope.PrintCouponList, function (k, v) {
                if (v.selected)
                    codes += v.code + '~';
            });

            if (codes == '~') {
                $rootScope.Toaster.Error("Please Select Atlest One Vouchers.");
                return;
            }
            $scope.PrintVoucherLabelbyCode(codes);
            //$timeout(function () {
            //    $rootScope.Download('CouponService/PrintGiftVoucherPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.GiftVoucher.id : id), 'voucherlable.pdf', 'application/pdf');
            //    return;
            //}, 10);
        }

        $scope.DeleteVoucherCode = function (item) {
            request = {
                "Item": { 'name': item.code },
                "SecurityToken": "String content"
            }
            // var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/DeleteVoucherCode",
                data: request
            }).success(function (result) {

                if (result) {
                    if (result.errormessage == 'undefined' || result.errormessage == null || result.errormessage == "") {
                        $rootScope.Toaster.Error("Successfully deleted");
                        $scope.PrintCouponList.splice($scope.PrintCouponList.indexOf(item), 1);
                    }
                    else
                        $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {

            });
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Edit") {
                $scope.VoucherId = item.voucherid;

                var dialog = ngDialog.open({
                    template: 'UpdateVoucherDate',
                    data: [$scope],
                    width: '600px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.parent = $scope.ngDialogData[0];
                        $scope.UpdateVoucherValidUp = function () {
                            $scope.parent.UpdateVoucherValidUp();
                            $scope.closeThisDialog(null);
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                            
                        }
                    }
                });
            }
        }
        
        $scope.UpdateVoucherValidUp = function () {
            var webrequest = {
                "item": {
                    'validto': $scope.VoucherNewValiddate,
                    'id': $scope.VoucherId
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "CouponService/UpdateVoucherValidDate",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Voucher date changed successfully.");
            }).error(function (result) {
            });
        }


        $scope.PrintVoucherLabelbyCode = function (code) {

            if (code.indexOf('~') < 0)
                code = '~' + code + '~';

            $timeout(function () {
                $rootScope.Download('CouponService/PrintGiftVoucherPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.GiftVoucher.id : id) + '/' + code, 'voucherlable.pdf', 'application/pdf');
                return;
            }, 10);
        }

        $scope.PrintBarcode = function () {
            var list = [];
            $.each($scope.PrintCouponList, function (k, v) {
                if (v.selected)
                    list.push({ 'id': v.id, 'code': v.code, 'value': v.amount, 'validupto': v.validupto, 'name': v.name });
            });
            if (list.length == 0) {
                $rootScope.ShowAlert('Voucher barcode', 'Please select the vouchers to be printed from the list.');
                return;
            }
            $rootScope.Prompt('Voucher Barcode', 'No Of Copies', function (count) {
                var requestitem = { 'copy': count, 'items': list, 'couponid': $scope.GiftVoucher.searchid };
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

            var request = {
                "Item": { 'coupenid': $scope.GiftVoucher.id },
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
                        singlerow.push('"' + result[i].code + '"');
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

        $scope.Delete = function () {

            if (typeof ($scope.CustomerSearchResult) == 'undefined' || $scope.CustomerSearchResult == null) return;
            if ($scope.CustomerSearchResult[0].id == 0) return;

            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete the all the selected voucher ?',
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
                                "Item": { 'id': $scope.CustomerSearchResult[0].id, 'version': $scope.CustomerSearchResult[0].version },
                                "SecurityToken": "String content"
                            }
                            // var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "CouponService/DeleteVoucher",
                                data: request
                            }).success(function (result) {

                                if (result) {
                                    if (result.errormessage == 'undefined' || result.errormessage == null || result.errormessage == "") {
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

        $scope.SearchVouchers = function () {
            request = {
                "Item": {
                    'companyid': $scope.GiftVoucher.companyid,
                    'locationid': $scope.GiftVoucher.locationid,
                    'issuedfrom': $scope.GiftVoucher.issuedfrom,
                    'issuedto': $scope.GiftVoucher.issuedto,
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CouponService/Search",
                data: srequest
            }).success(function (result) {
                $scope.GiftVoucherList = result;
            }).error(function (result) {
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
                //}).success(function(result) {

                //}).error(function(error) {

                //});
            })
        }
    }]);