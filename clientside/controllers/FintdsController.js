var app = angular.module('InfinityLab');
app.controller('FintdsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$filter', '$timeout', 'cntxService', '$state',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $filter, $timeout, cntxService, $state) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.filteredtype = '';
        $scope.Location = null;
        $scope.Company = null;
        $scope.TDSLedgers = null;
        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        restService.Get('ReferencelistService/TDSLEDGERS', true).then(function (results) {
            $scope.TDSLedgers = results;
        });

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            if ($scope.SearchRequest && $scope.SearchRequest.locations != null) {
                $.each($scope.SearchRequest.locations, function (k, v) {
                    var exist = $scope.Location.find(function (item) {
                        return item.id == v;
                    });
                    if (exist && exist != null)
                        exist.view = true;
                });
            }
        });

        $scope.PurchaseType = null;
        restService.Get('ReferencelistService/FULL-PURCHASETYPE', true).then(function (results) {
            $scope.PurchaseType = results;
        });

        $scope.vouchertypes = [
            { 'id': 1, 'name': 'Payment' },
            { 'id': 2, 'name': 'Receipt' },

            { 'id': 3, 'name': 'Purchase' },
            { 'id': 4, 'name': 'Sales' },
            { 'id': 5, 'name': 'Credit Note' },
            { 'id': 6, 'name': 'Debit Note' },

            { 'id': 7, 'name': 'Contra' },
            { 'id': 8, 'name': 'Journal Cr' },

            { 'id': 9, 'name': 'Cash Deposit' },
            { 'id': 10, 'name': 'Cash Widhrawal' },
            { 'id': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': 12, 'name': 'Petty Cash to Main Cash' },
            { 'id': 13, 'name': 'Cash Counter Expense' },

            { 'id': 14, 'name': 'Advance Payment' },

            { 'id': 15, 'name': 'Petty Cash Expense' },
            { 'id': 16, 'name': 'Journal Dr' },
            { 'id': 99, 'name': 'EOD Posting' }
        ];


        $scope.ItemTypes = [{ 'id': 1, 'name': 'Capital' },
        { 'id': 2, 'name': 'Fixed asset' },
        { 'id': 19, 'name': 'Current asset' },
        { 'id': 3, 'name': 'Liablities' },
        { 'id': 20, 'name': 'Current Liablities' },
        { 'id': 4, 'name': 'Direct Income' },
        { 'id': 5, 'name': 'Direct Expense' },
        { 'id': 6, 'name': 'Cash-In-Hand' },
        { 'id': 7, 'name': 'Bank Account' },
        { 'id': 8, 'name': 'Stock-In-Hand' },
        { 'id': 9, 'name': 'Indirect Incomes' },
        { 'id': 10, 'name': 'Indirect Expenses' },
        { 'id': 11, 'name': 'Sundry Debtors' },
        { 'id': 12, 'name': 'Sundry Creditors' },
        { 'id': 13, 'name': 'Petty Cash' },
        { 'id': 14, 'name': 'Sales' },
        { 'id': 15, 'name': 'Purchase' },
        { 'id': 16, 'name': 'Tax & Duties' },
        { 'id': 17, 'name': 'Petty Cash Direct Expense' },
        { 'id': 18, 'name': 'Petty Cash In-Direct Expense' },
        { 'id': 30, 'name': 'Rounding' }];

        $scope.Search = function () {

            $scope.SearchRequest.ledgers = [];
            $.each($scope.TDSLedgers, function (k, v) {
                if (v.view)
                    $scope.SearchRequest.ledgers.push(v.id);
            })

            $scope.SearchRequest.locations = [];
            $.each($scope.Location, function (k, v) {
                if (v.view)
                    $scope.SearchRequest.locations.push(v.id);
            })

            $scope.SearchRequest.vtypes = [];
            $.each($scope.vouchertypes, function (k, v) {
                if (v.view)
                    $scope.SearchRequest.vtypes.push(v.id);
            })

            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "VoucherService/TDSReport",
                data: srequest
            }).success(function (result) {
                $scope.LastResult = result;
                $scope.ConfigVoucherGrid(result);
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Finance TDS Analyser'
            infygrid.ExportXLSX(infygrid.grid, title, subtitle);

            //var title = $rootScope.UserInfo.CompanyName;
            //var subtitle = 'Finance Analyser'
            //$scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            //if ($scope.exportHref != null) {
            //    $timeout(function () {
            //        var link = document.createElement('a');
            //        link.download = filename + ".xls";
            //        link.href = $scope.exportHref;
            //        document.body.appendChild(link);
            //        link.click();
            //        link.remove();
            //    }, 100);
            //}
        }

        $scope.ConfigVoucherGrid = function (result) {
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            var column = igrid.AddFormatedColumn('date', 'Voucher Date', 80, 'dd-MM-yyyy');
            column.getValue = function (item, col) {
                return new Date(item.date);
            }

            igrid.AddSimpleColumn('no', 'Voucher No', 80);
            var column = igrid.AddCustomColumn('type', 'Voucher Type', 150, function (item, col) {
                if ($scope.PurchaseType &&
                    $scope.PurchaseType.length > 0 &&
                    item.type == 3 && item.ptype && item.ptype > 0)
                    return $scope.SearchRef(item.ptype, $scope.PurchaseType, 'Purchase Voucher');

                return $scope.SearchRef(item.type, $scope.vouchertypes, 'Petty Cash');
            });

            igrid.AddSimpleColumn('sname', 'Source Name', 200);
            igrid.AddSimpleColumn('name', 'Party Name', 200);
            igrid.AddSimpleColumn('pan', 'PAN', 200);

            var column = igrid.AddCustomColumn('dr', 'Debit', 100, function (item, col) {
                return $filter('IndianNumber')(item.dr, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.dr;
            }

            var column = igrid.AddCustomColumn('cr', 'Credit', 100, function (item, col) {
                return $filter('IndianNumber')(item.cr, 2);
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.cr;
            }


            igrid.AddSimpleColumn('tdsledgername', 'TDS Name', 200);

            var column = igrid.AddCustomColumn('tdspercent', 'TDS %', 80, function (item, col) {
                return $filter('IndianNumber')(item.tdspercentage, 2);
            });
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                return item.tdspercentage;
            }

            var column = igrid.AddCustomColumn('tds', 'TDS', 80, function (item, col) {
                return $filter('IndianNumber')(item.tds, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.tds;
            }

            var column = igrid.AddCustomColumn('cesspercent', 'CESS %', 80, function (item, col) {
                return $filter('IndianNumber')(item.cesspercentage, 2);
            });
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                return item.cesspercentage;
            }

            var column = igrid.AddCustomColumn('cess', 'CESS', 80, function (item, col) {
                return $filter('IndianNumber')(item.cess, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.cess;
            }

            var column = igrid.AddCustomColumn('cesspercent', 'SURCHARGE %', 80, function (item, col) {
                return $filter('IndianNumber')(item.surchargepercentage, 2);
            });
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                return item.surchargepercentage;
            }

            var column = igrid.AddCustomColumn('surcharge', 'SURCHARGE', 80, function (item, col) {
                return $filter('IndianNumber')(item.surcharge, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.surcharge;
            }            

            $scope.selectedcount = 0;
            igrid.EnableTotal();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.tsummary, '_id');
            $scope.SearchRequest.types
        }

        $scope.selectedcount = 0;
        $scope.EditVoucher = function () {
            $timeout(function () {
                var row = $scope.igrid.GetSelectedRows()[0];
                $state.go('voucher', { 'voucher': { 'id': row.voucherid } });
            }, 10);
        }

        $scope.SelectAll = function (list, value) {
            $.each(list, function (k, v) {
                v.view = value;
            })
        }

        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings &&
                $scope.PurchaseType && $scope.PurchaseType != null) {
                $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
                $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
                $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                $scope.SearchRequest = $scope.LocalContext.Get('$scope.SearchRequest', false);
                $scope.LastResult = $scope.LocalContext.Get('$scope.LastResult', false);
                if ($scope.LastResult && $scope.LastResult != null) {
                    if ($scope.SearchRequest.types &&
                        $scope.SearchRequest.types != null) {

                        $.each($scope.SearchRequest.types, function (k, v) {
                            var exist = $scope.ItemTypes.find(function (item) {
                                return item.id == v;
                            });
                            if (exist && exist != null)
                                exist.view = true;
                        });
                    }

                    if ($scope.SearchRequest.vtypes &&
                        $scope.SearchRequest.vtypes != null) {
                        $.each($scope.SearchRequest.vtypes, function (k, v) {
                            var exist = $scope.vouchertypes.find(function (item) {
                                return item.id == v;
                            });
                            if (exist && exist != null)
                                exist.view = true;
                        });
                    }

                    if ($scope.SearchRequest.locations &&
                        $scope.SearchRequest.locations != null) {
                        if ($scope.Location && $scope.Location != null) {
                            $.each($scope.SearchRequest.locations, function (k, v) {
                                var exist = $scope.Location.find(function (item) {
                                    return item.id == v;
                                });
                                if (exist && exist != null)
                                    exist.view = true;
                            });
                        }
                    }

                    $scope.ShowResult($scope.LastResult);
                }
            } else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 10);
            }
        }

        $scope.LocalContext = cntxService.CreatePageContext('fintdsanalyser');
        $scope.$on(
            "$destroy",
            function (event) {
                $scope.LocalContext.Post('$scope.SearchRequest', $scope.SearchRequest, false);
                $scope.LocalContext.Post('$scope.LastResult', $scope.LastResult, false);
            }
        );

        $timeout(function () {
            $scope.LoadAppSetting();

        }, 10);
    }]);
