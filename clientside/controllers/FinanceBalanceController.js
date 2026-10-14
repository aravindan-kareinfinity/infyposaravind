var app = angular.module('InfinityLab');
app.controller('FinanceBalanceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$stateParams', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $stateParams, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.ReportType = $stateParams.type;
        $scope.SearchRef = restService.SearchReference;
        $scope.Years = [];
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
        });
        $scope.Locations = [];
        restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.FinanceReportTitle = "";
        $scope.SearchCondition = {};
        var endYear = (new Date()).getFullYear();
        for (var i = 0; i < 10; i++)
            $scope.Years.push({ 'id': endYear - i, 'name': endYear - i });
        if ((new Date()).getMonth() > 2)
            $scope.SearchCondition.year = endYear;
        else
            $scope.SearchCondition.year = endYear - i;

        if ($location.path().indexOf('tradingaccount') > 0)
            $scope.FinanceReportTitle = 'Trading Account';
        else if ($location.path().indexOf('profitandloss') > 0)
            $scope.FinanceReportTitle = 'Profit & Loss Account';
        else
            $scope.FinanceReportTitle = 'Balance Sheet';

        $scope.ExportXLSX = function () {
            if ($scope.igrid == undefined || $scope.igrid == null) return;

            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = $scope.FinanceReportTitle;
            $scope.igrid.ExportXLSX($scope.igrid.grid, title, subtitle);
        }

        $scope.SearchFinanceReport = function (ledgeropening) {

            var servicename = 'CurrentBalanceSheet';
            if ($location.path().indexOf('tradingaccount') > 0)
                servicename = 'CurrentTradingAccount';
            if ($location.path().indexOf('profitandloss') > 0)
                servicename = 'CurrentProfitAndLossAccount';

            if ($scope.SearchCondition.locationid == undefined || $scope.SearchCondition.locationid == null)
                $scope.SearchCondition.locationid = -1;

            if (ledgeropening == undefined || ledgeropening == null)
                ledgeropening = false;

            var from = $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy');
            var to = $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');

            if (from == '01-01-0001' || to == '01-01-0001') return;
            if ($scope.SearchCondition.companyid == undefined || $scope.SearchCondition.companyid == null) return;

            $scope.ReportList = restService.Get('VoucherService/' + servicename + '/' + $scope.SearchCondition.companyid +
                '/' + $scope.SearchCondition.locationid + '/' + from + '/' + to + '/' + ledgeropening, true).then(function (results) {

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    if (servicename == 'CurrentBalanceSheet') {
                        igrid.AddSimpleColumn('DebitDisplay', 'Asset', 200);
                        var column = igrid.AddCustomColumn('DebitHeader', 'Header', 100, function (item, col) {
                            return item.DebitHeader;
                        });

                        var column = igrid.AddCustomColumn('ob-debit', 'Opening', 100, function (item, col) {
                            if (item.DebitOBCB && item.DebitOBCB != null) {
                                if (item.DebitOBCB.OpeningDR > item.DebitOBCB.OpeningCR) {
                                    var val = item.DebitOBCB.OpeningDR - item.DebitOBCB.OpeningCR;
                                    if (Math.round(val) > 0) {
                                        return "<span style='display:block;width:100%;'>" + Math.round(val) + ' Dr' + "</span>";
                                    }
                                } else if (item.DebitOBCB.OpeningDR < item.DebitOBCB.OpeningCR) {
                                    var val = item.DebitOBCB.OpeningCR - item.DebitOBCB.OpeningDR;
                                    if (Math.round(val) > 0) {
                                        return "<span style='display:block;width:100%;' class='igridbigrowswithred'>" + Math.round(val) + ' Cr' + "</span>";
                                    }
                                }
                            }
                            return '';
                        });
                        column.cssClass = 'rightalign';
                        column.total = true;
                        column.getValue = function (item, col) {
                            if (item.DebitOBCB && item.DebitOBCB != null) {
                                if (item.DebitOBCB.OpeningDR > item.DebitOBCB.OpeningCR) {
                                    var val = item.DebitOBCB.OpeningDR - item.DebitOBCB.OpeningCR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val);
                                    }
                                } else if (item.DebitOBCB.OpeningDR < item.DebitOBCB.OpeningCR) {
                                    var val = item.DebitOBCB.OpeningCR - item.DebitOBCB.OpeningDR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val)*-1;
                                    }
                                }
                            }
                            return 0;
                        }

                        var column = igrid.AddCustomColumn('Debit', 'Amount', 100, function (item, col) {
                            if (item.Debit && item.Debit != 0) {
                                if (item.Debit > 0)
                                    return item.Debit + ' Dr';
                                return "<span style='display:block;width:100%;' class='igridbigrowswithred'>" + Math.round((item.Debit * -1)) + ' Cr' + "</span>";
                            }
                            return '';
                        });
                        column.getValue = function (item, col) {
                            return item.Debit
                        }
                        column.cssClass = 'rightalign';
                        column.total = true;

                        var column = igrid.AddCustomColumn('cb-debit', 'Closing', 100, function (item, col) {
                            if (item.DebitOBCB && item.DebitOBCB != null) {
                                if (item.DebitOBCB.ClosingDR > item.DebitOBCB.ClosingCR) {
                                    var val = item.DebitOBCB.ClosingDR - item.DebitOBCB.ClosingCR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val) + ' Dr';
                                    }
                                } else if (item.DebitOBCB.ClosingDR < item.DebitOBCB.ClosingCR) {
                                    var val = item.DebitOBCB.ClosingCR - item.DebitOBCB.ClosingDR;
                                    if (Math.round(val) > 0) {
                                        return "<span style='display:block;width:100%;' class='igridbigrowswithred'>" + Math.round(val) + ' Cr' + "</span>";
                                    }
                                }
                            }
                            return '';
                        });
                        column.cssClass = 'rightalign';
                        column.total = true;
                        column.getValue = function (item, col) {
                            if (item.DebitOBCB && item.DebitOBCB != null) {
                                if (item.DebitOBCB.ClosingDR > item.DebitOBCB.ClosingCR) {
                                    var val = item.DebitOBCB.ClosingDR - item.DebitOBCB.ClosingCR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val);
                                    }
                                } else if (item.DebitOBCB.ClosingDR < item.DebitOBCB.ClosingCR) {
                                    var val = item.DebitOBCB.ClosingCR - item.DebitOBCB.ClosingDR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val)*-1;
                                    }
                                }
                            }
                            return 0;
                        }
                        igrid.AddSimpleColumn('CreditDisplay', 'Liabilities', 200);
                        var column = igrid.AddCustomColumn('CreditHeader', 'Header', 100, function (item, col) {
                            return item.CreditHeader;
                        });

                        var column = igrid.AddCustomColumn('ob-credit', 'Opening', 100, function (item, col) {
                            if (item.CreditOBCB && item.CreditOBCB != null) {
                                if (item.CreditOBCB.OpeningDR > item.CreditOBCB.OpeningCR) {
                                    var val = item.CreditOBCB.OpeningDR - item.CreditOBCB.OpeningCR;
                                    if (Math.round(val) > 0) {
                                        return "<span style='display:block;width:100%;' class='igridbigrowswithred'>" + Math.round(val) + ' Dr' + "</span>";
                                    }
                                } else if (item.CreditOBCB.OpeningDR < item.CreditOBCB.OpeningCR) {
                                    var val = item.CreditOBCB.OpeningCR - item.CreditOBCB.OpeningDR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val) + ' Cr';
                                    }
                                }
                            }
                            return '';
                        });
                        column.cssClass = 'rightalign';
                        column.total = true;
                        column.getValue = function (item, col) {
                            if (item.CreditOBCB && item.CreditOBCB != null) {
                                if (item.CreditOBCB.OpeningDR > item.CreditOBCB.OpeningCR) {
                                    var val = item.CreditOBCB.OpeningDR - item.CreditOBCB.OpeningCR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val)*-1;
                                    }
                                } else if (item.CreditOBCB.OpeningDR < item.CreditOBCB.OpeningCR) {
                                    var val = item.CreditOBCB.OpeningCR - item.CreditOBCB.OpeningDR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val);
                                    }
                                }
                            }
                            return 0;
                        }

                        var column = igrid.AddCustomColumn('Credit', 'Amount', 100, function (item, col) {
                            if (item.Credit && item.Credit != 0) {
                                if (item.Credit > 0)
                                    return item.Credit + ' Cr';
                                return "<span style='display:block;width:100%;' class='igridbigrowswithred'>" + Math.round((item.Credit * -1)) + ' Dr' + "</span>";
                            }
                            return '';
                        });
                        column.getValue = function (item, col) {
                            return item.Credit
                        }
                        column.cssClass = 'rightalign';
                        column.total = true;

                        var column = igrid.AddCustomColumn('cb-credit', 'Closing', 100, function (item, col) {
                            if (item.CreditOBCB && item.CreditOBCB != null) {
                                if (item.CreditOBCB.ClosingDR > item.CreditOBCB.ClosingCR) {
                                    var val = item.CreditOBCB.ClosingDR - item.CreditOBCB.ClosingCR;
                                    if (Math.round(val) > 0) {
                                        return "<span style='display:block;width:100%;' class='igridbigrowswithred'>" + Math.round(val) + ' Dr' + "</span>";
                                    }
                                } else if (item.CreditOBCB.ClosingDR < item.CreditOBCB.ClosingCR) {
                                    var val = item.CreditOBCB.ClosingCR - item.CreditOBCB.ClosingDR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val) + ' Cr';
                                    }
                                }
                            }
                            return '';
                        });
                        column.cssClass = 'rightalign';
                        column.total = true;
                        column.getValue = function (item, col) {
                            if (item.CreditOBCB && item.CreditOBCB != null) {
                                if (item.CreditOBCB.ClosingDR > item.CreditOBCB.ClosingCR) {
                                    var val = item.CreditOBCB.ClosingDR - item.CreditOBCB.ClosingCR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val)*-1;
                                    }
                                } else if (item.CreditOBCB.ClosingDR < item.CreditOBCB.ClosingCR) {
                                    var val = item.CreditOBCB.ClosingCR - item.CreditOBCB.ClosingDR;
                                    if (Math.round(val) > 0) {
                                        return Math.round(val) ;
                                    }
                                }
                            }
                            return 0;
                        }
                    }
                    else {
                        igrid.AddSimpleColumn('DebitDisplay', 'Particulars', 100);
                        column = igrid.AddFormatedColumn('Debit', 'Debit', 100, 'N2', 2);
                        column.cssClass = 'rightalign';
                        column.total = true;

                        igrid.AddSimpleColumn('CreditDisplay', 'Particulars', 100);
                        column = igrid.AddFormatedColumn('Credit', 'Credit', 100, 'N2', 2);
                        column.cssClass = 'rightalign';
                        column.total = true;
                    }

                    igrid.EnableTotal();
                    igrid.Initialize();

                    igrid.grid.onDblClick.subscribe(function (e, args) {
                        var fild = args.grid.getColumns()[args.cell].id;
                        item = args.grid.getDataItem(args.row);
                        if (item && item[fild] && item[fild] != null) {
                            if (item[fild] == 'Opening Stock' || item[fild] == 'Closing Stock') {
                                $scope.RefreshReportWithOpeningBalance();
                            }
                        }
                    });


                    //igrid.dataView.getItemMetadata = function (row) {
                    //    var item = igrid.dataView.getItem(row);
                    //    if (item.Credit < 0 || item.Debit < 0) {
                    //        item.style = 'igridbigrowswithred';
                    //        return { cssClasses: 'igridbigrowswithred' };
                    //    }
                    //};
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(results.DisplayList, 'id');

                });
        }


        $scope.RefreshReportWithOpeningBalance = function () {
            if (from == '01-01-0001' || to == '01-01-0001') return;
            if ($scope.SearchCondition.companyid == undefined || $scope.SearchCondition.companyid == null) return;
            var from = $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy');
            var to = $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');
            var request = {
                'Item': {
                    'companyid': $scope.SearchCondition.companyid,
                    'fromdate': from,
                    'todate': to
                }
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LedgerstockopeningService/FindStockOpening",
                data: srequest
            }).success(function (result) {


            });
        }

        $timeout(function () {
            $scope.SearchFinanceReport();
        }, 10);

    }]);


