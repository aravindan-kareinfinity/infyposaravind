
var app = angular.module('InfinityLab');

app.controller('RewardManagerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;

        $scope.Reasons = [{ 'id': 0, 'name': 'OPENING BALANCE' }, { 'id': 1, 'name': 'MIGRATION' }, { 'id': 2, 'name': 'MERGE' }, { 'id': 3, 'name': 'ADJUSTMENT' }];


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
            { denomination: 100, qty: 0, amount: 0 }]
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

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            customerid: ""
        }

        $scope.AmountChanged = function (item) {
            var oldValue = item.amount;
            item.amount = isNaN(item.denomination * item.qty) ? 0 : item.denomination * item.qty;
            $scope.GiftVoucher.vouchervalue = $scope.GiftVoucher.vouchervalue - oldValue + item.amount;

        };

        $scope.Save = function () {

            request = {
                "Item": $scope.GiftVoucher,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CouponService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("GiftVoucher Saved Successfully.!");

                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

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

        $scope.SearchCustomer = function () {
            if ($scope.GiftVoucher.mobileno == null || $scope.GiftVoucher.mobileno == "") {
                $scope.GiftVoucher.name = "Cash";
                return;
            }
            restService.Get('CustomerService/PhoneNumber?id=' + $scope.GiftVoucher.mobileno, true).then(function (results) {
                $scope.GiftVoucher.name = results.name;

                $scope.GiftVoucher.customerid = results.id;
                $scope.SearchVoucherList();
            });
        }

        $scope.SelectAllChanged = function () {
            $.each($scope.SearchResult, function (k, v) {
                v.selected = $scope.SearchCondition.SelectAll;
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

        $scope.ConvertCoupon = function () {
            var dialog = ngDialog.open({
                template: 'ConvertCoupon',
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
                    }
                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                }
            });
        }

        $scope.AdjustLoyalty = function () {
            var dialog = ngDialog.open({
                template: 'AdjustLoyalty',
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
                    }
                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                }
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
            $rootScope.Handlers.SelectPrinter('EMPLOYEECODE', function (printdata) {
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
                    url: "EmployeeService/PrintBarcode",
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

                $rootScope.Sent2Printer(printdocument, printerData);

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
    }]);