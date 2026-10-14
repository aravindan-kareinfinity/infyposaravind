
var app = angular.module('InfinityLab');

app.controller('TallyIntegrationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', 'localStorageService','ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, localStorageService, ngDialog) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;
        $scope.totalvalue = 0;
        $scope.reporttype = "PURCHASE";
        $scope.ReportType = [{ 'code': 'PURCHASE', 'name': 'Purchase' },
            { 'code': 'PURCHASERETURN', 'name': 'Purchase Return' },
            { 'code': 'SALES', 'name': 'Sales' },
            { 'code': 'PAYMENT', 'name': 'Supplier Payment' },
            { 'code': 'CREDITPAYMENT', 'name': 'Customer Payment' },
            { 'code': 'BOMPURCHASE', 'name': 'BOM Purchase' },
            { 'code': 'BOMPURCHASERETURN', 'name': 'BOM Purchase Return' },
            { 'code': 'PETTYCASH', 'name': 'Petty Cash' },
        ];

        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });

        //$scope.Customer = restService.Get('ReferencelistService/CUSTOMER', true).then(function (results) {
        //    $scope.Customer = results;
        //});

        $scope.city = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.city = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });

        $scope.AllLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.AllLocation = results;
            $scope.showentrylocation = $rootScope.UserInfo.appsettings.system_showroompurchase || false;
        });
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
            var exist = false;
            $.each($scope.Sizes, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.Sizes.push({ id: -2, name: 'Jump' });
                $scope.Sizes.push({ id: -3, name: 'Cut' });
            }

        });
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
        });
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
        });

        $scope.SearchCondition = {
            mode: 1,
            fromdate: new Date(),
            todate: new Date(),
            companyid: 0,
            supplierid: 0
        };

        $scope.ChangeReport = function () {
            $scope.PurchaseResult = [];
            $scope.PurchaseReturnResult = [];
            $scope.SalesResult = [];
        }

        $scope.PurchaseResult = [];
        $scope.PurchaseReturnResult = [];
        $scope.SalesResult = [];

        $scope.taxdisplay = function (taxtype, item) {

            var ditem = item.taxbreakup.find(function (searchitem) {
                return searchitem.taxpercent == taxtype.percentage;
            });

            if (typeof (ditem) == 'undefinied' || ditem == null)
                return null;

            if (taxtype.type == 1)
                return ditem.taxableamount;
            else
                return ditem.taxamount;
        };

        $scope.CurrentVoucher = {};

        $scope.ShowLedgers = function(item)
        {
            $scope.ledgers = item.ledgers;
            $scope.narration = item.narration;
        }

        $scope.SearchPayment = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreatePaymentVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $scope.totalvalue += v.ledgers[0].Value;
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.SearchPurchase = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreatePurchaseVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $scope.totalvalue += v.ledgers[0].Value;
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.SearchBOMPurchase = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreateBOMPurchaseVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $scope.totalvalue += v.ledgers[0].Value;
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.selectallchanged = function (selected) {
            $.each($scope.Vouchers, function (k, v) {
                v.selected = selected;
            })
        }

        $scope.SearchPurchaseReturn = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreatePurchaseReturnVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $.each($scope.Vouchers, function (k, v) {
                        $.each(v.ledgers, function (kk, vv) {
                            if (vv.Value > 0)
                                $scope.totalvalue += vv.Value;
                        })
                    });
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.SearchBOMPurchaseReturn = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreateBOMPurchaseReturnVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $.each($scope.Vouchers, function (k, v) {
                        $.each(v.ledgers, function (kk, vv) {
                            if (vv.Value > 0)
                                $scope.totalvalue += vv.Value;
                        })
                    });
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.SearchCreditPayment = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreateCreditPaymentVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $.each($scope.Vouchers, function (k, v) {
                        $.each(v.ledgers, function (kk, vv) {
                            if (vv.Value > 0)
                                $scope.totalvalue += vv.Value;
                        })
                    });
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.SearchPettyCash = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreatePettycashVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $.each($scope.Vouchers, function (k, v) {
                        $.each(v.ledgers, function (kk, vv) {
                            if (vv.Value > 0)
                                $scope.totalvalue += vv.Value;
                        })
                    });
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.SearchSales = function () {
            $scope.totalvalue = 0;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TallyService/CreateSalesVouchers",
                data: srequest
            }).success(function (result) {
                $scope.CurrentVoucher = result;
                $scope.Vouchers = result.vouchers;
                $.each($scope.Vouchers, function (k, v) {
                    $.each(v.ledgers, function (kk, vv) {
                        if (vv.Value>0)
                            $scope.totalvalue += vv.Value;
                    })
                });
                $scope.LocationChanged();
            }).error(function (result) {
            });
        }

        $scope.LocationChanged = function () {

            var locationid = $scope.reporttype == 'PURCHASE' ? $scope.SearchCondition.entrylocationid : $scope.SearchCondition.locationid;
            if (locationid && locationid > 0) {
                var location = $scope.AllLocation.find(function (item) {
                    return item.id == locationid;
                });
                if (location && location.id > 0) {
                    if (location.additionalinfo && location.additionalinfo.locationconfig) {

                        if (location.additionalinfo.locationconfig.tallyserver && location.additionalinfo.locationconfig.tallycompany != '') {
                            $scope.CurrentVoucher.company = location.additionalinfo.locationconfig.tallycompany;
                        }

                        if (location.additionalinfo.locationconfig.tallyserver && location.additionalinfo.locationconfig.tallyserver != '') {
                            $scope.CurrentVoucher.machine = location.additionalinfo.locationconfig.tallyserver.split(':')[0];
                            $scope.CurrentVoucher.port = location.additionalinfo.locationconfig.tallyserver.split(':')[1];
                        }
                    }
                }
            }

            if ($scope.Vouchers &&
                $scope.Vouchers != null &&
                $scope.Vouchers.length > 0) {
                $scope.selectall = true;
                $.each($scope.Vouchers, function (k, v) {
                    v.selected = true;
                })
            }

        }

        $scope.Transfer2Tally4Delete = function()
        {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                $rootScope.Toaster.Error('Please select the printer server');
                return;
            }

            var request = {
                'url': 'http://' + $scope.CurrentVoucher.machine + ':' + $scope.CurrentVoucher.port,
                'groups': [],
                'ledgers': [],
                'vouchers': []
            };

            $.each($scope.CurrentVoucher.vouchers, function (k, v) {
                if(v.selected)
                    request.vouchers.push({ 'id': k, 'xml': v.dxml });
            });
            if (printerData.IsWebPrint) {
                $rootScope.WebPrint.Transfer2Tally(request, function () { });
            }
            else {
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: printerData.service.replace('Print', 'Transfer2Tally'),
                    data: srequest
                }).success(function (result) {
                    $rootScope.Toaster.Info('Transfer success');
                }).error(function (error) {
                    $rootScope.Toaster.Error('Transfer failed - ' + error);
                });
            }
        }

        $scope.ShowCustomerDialog = function () {


            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
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
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
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
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
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
                        $scope.setFocuscustomer_name();
                    }, 10);

                    $scope.contactno = $scope.ngDialogData[0].SearchCondition.customername;
                    if (typeof ($scope.contactno) != 'undefined' && $scope.contactno != null) {
                        if ($scope.contactno.length != 10 || $scope.ngDialogData[0].GetNumber($scope.contactno) <= 0) {
                            $scope.name = $scope.contactno;
                            $scope.contactno = '';
                        }
                        $scope.Search(true);
                    }

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {
                        $scope.SearchCondition.customerid = data.value.id;
                        $scope.SearchCondition.customername = data.value.name;
                        $scope.SearchCondition.customerid = data.value.id;
                        $scope.SearchCondition.customername = data.value.name;
                        //$scope.SearchItems();
                    }
                }
            });
        }

        $scope.Transfer2Tally = function()
        {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                $rootScope.Toaster.Error('Please select the printer');
                return;
            }

            var request = {
                'url': 'http://' + $scope.CurrentVoucher.machine + ':' + $scope.CurrentVoucher.port,
                'groups': [],
                'ledgers': [],
                'vouchers': []
            };

            $.each($scope.CurrentVoucher.ledgers, function (k, v) {
                request.ledgers.push({ 'id': k, 'xml': v.xml });
            });

            $.each($scope.CurrentVoucher.groups, function (k, v) {
                request.groups.push({ 'id': k, 'xml': v.xml });
            });

            $.each($scope.CurrentVoucher.vouchers, function (k, v) {
                if (v.selected)
                    request.vouchers.push({ 'id': k, 'xml': v.xml });
            });

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: printerData.service.replace('Print', 'Transfer2Tally'),
                data: srequest
            }).success(function(result) {
                $rootScope.Toaster.Info('Transfer success');

            }).error(function(error) {
                $rootScope.Toaster.Error('Transfer failed - ' + error);
            });
        }


 }]);