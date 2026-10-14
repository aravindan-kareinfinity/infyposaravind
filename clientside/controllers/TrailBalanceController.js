var app = angular.module('InfinityLab');
app.controller('TrailBalanceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', '$timeout', 'Excel', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.Company = [];
        $scope.Locations = [];
        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
        });
        restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.SearchRef = restService.SearchReference;
        $scope.Years = [];
        $scope.SearchCondition = {};
        var endYear = (new Date()).getFullYear();
        for (var i = 0; i < 10; i++)
            $scope.Years.push({ 'id': endYear - i, 'name': endYear - i });
        if ((new Date()).getMonth() > 2)
            $scope.SearchCondition.year = endYear;
        else
            $scope.SearchCondition.year = endYear - i;


        $scope.exporttoExcel = function (infygrid, filename) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Trial Balance - ' + subtitle;
            infygrid.ExportXLSX(infygrid.grid, title, subtitle);
        }


        $scope.ConfigGrid = function (results) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            var column = igrid.AddFormatrerColumn('grouping', "<span class='infygridtoggle expand'></span>&nbsp;", 40,
                function (row, cell, value, columnDef, dataContext) {
                    var spacer = "<span style='display:inline-block;height:1px;width:8px'></span>";
                    if (dataContext["itemlevel"] == 0) {
                        if (dataContext.collapsed) {
                            return spacer + " <span class='infygridtoggle expand'></span>&nbsp;";
                        } else {
                            return spacer + " <span class='infygridtoggle collapse'></span>&nbsp;";
                        }
                    } else {
                        return spacer + " <span class='infygridtoggle'></span>&nbsp;";
                    }
                });
            column.sortable = false;

            var column = igrid.AddCustomColumn('displayname', 'Particulars', 350, function (item, col) {
                return '&nbsp;'.repeat(item.itemlevel * 10) + item.displayname;
            });

            var column = igrid.AddCustomColumn('parentgroup', 'Header', 350, function (item, col) {
                if (item.parentgroup == undefined || item.parentgroup == null || item.parentgroup == '')
                    return item.coretype;
                return item.parentgroup;
            });

            column = igrid.AddCustomColumn('opening', 'Opening', 120, function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return '';
                if (item.OpeningDR == 0 && item.OpeningCR == 0) return '';
                if (item.OpeningDR > item.OpeningCR)
                    return $filter('IndianNumber')(item.OpeningDR - item.OpeningCR) + ' Dr';
                else
                    return $filter('IndianNumber')(item.OpeningCR - item.OpeningDR) + ' Cr';
            });
            column.cssClass = 'rightalign';

            /*column = igrid.AddCustomColumn('DebitChild', '', 80, function (item, col) {
                if (item.itemlevel > 0) {
                    return item.debit > 0 ? item.debit : '';
                }
                return '';
            });
            column.cssClass = 'rightalign';
            //column.total = true;
            column.getValue = function (item, col) {
                return item.itemlevel > 0 && item.debit > 0 ? item.debit : 0;
            }*/


            column = igrid.AddCustomColumn('Debit', 'Debit', 120, function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return '';

                //if (item.itemlevel == 0) {
                return item.debit != 0 ? $filter('IndianNumber')(item.debit) : '';
                //}
                //return '';
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return 0;
                return item.debit != 0 ? item.debit : 0;
            }

            //column = igrid.AddCustomColumn('Debit', 'Debit', 350, function (item, col) {
            //    if (item.itemlevel == 0) {
            //        return item.debit > 0 ? item.debit : '';
            //    }
            //    return item.balancemode == 'Cr' ? '' : (item.balance.toString() + '&nbsp;'.repeat(item.itemlevel * 20));
            //});



            //column = igrid.AddCustomColumn('CreditChild', '', 80, function (item, col) {
            //    if (item.itemlevel > 0) {
            //        return item.credit > 0 ? item.credit : '';
            //    }
            //    return '';
            //});
            //column.cssClass = 'rightalign';
            ////column.total = true;
            //column.getValue = function (item, col) {
            //    return item.itemlevel > 0 && item.credit > 0 ? item.credit : 0;
            //}

            column = igrid.AddCustomColumn('Credit', 'Credit', 120, function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return '';
                return item.credit != 0 ? $filter('IndianNumber')(item.credit) : '';
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return 0;
                return item.credit != 0 ? item.credit : 0;
            }

            column = igrid.AddCustomColumn('closingcr', 'Closing Dr', 120, function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return '';
                if (item.ClosingDR == 0 && item.ClosingCR == 0) return '';
                if (item.ClosingDR > item.ClosingCR)
                    return $filter('IndianNumber')(item.ClosingDR - item.ClosingCR) ;
                return '';
            });
            column.total = true;
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return 0;
                if (item.ClosingDR == 0 && item.ClosingCR == 0) return 0;
                if (item.ClosingDR > item.ClosingCR)
                    return (item.ClosingDR - item.ClosingCR);
                return 0;
            }

            column = igrid.AddCustomColumn('closing', 'Closing Cr', 120, function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return '';
                if (item.ClosingDR == 0 && item.ClosingCR == 0) return '';
                if (item.ClosingDR < item.ClosingCR)
                    return $filter('IndianNumber')(item.ClosingCR - item.ClosingDR);
                return '';
            });
            column.total = true;
            column.getValue = function (item, col) {
                if (item.itemlevel == 0 &&
                    !item.collapsed)
                    return 0;
                if (item.ClosingDR == 0 && item.ClosingCR == 0) return 0;
                if (item.ClosingDR < item.ClosingCR)
                    return (item.ClosingCR - item.ClosingDR);
                return 0;
            }
            column.cssClass = 'rightalign';

            //column = igrid.AddCustomColumn('Credit', 'Credit', 350, function (item, col) {
            //    if (item.itemlevel == 0) {
            //        return item.credit > 0 ? item.credit : '';
            //    }
            //    return item.balancemode == 'Dr' ? '' : (item.balance.toString() + '&nbsp;'.repeat(item.itemlevel * 20));
            //});
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'grouping') {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }
                    if ($(e.target).hasClass('expand')) {
                        $(e.target).removeClass('expand');
                        $(e.target).addClass('collapse');
                        $.each($scope.CurrentSearchResult, function (k, v) {
                            v.show = true
                            v.collapsed = false;
                        });
                        //for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                        //    var item = igrid.grid.getDataItem(i);
                        //    item.indent = item.itemlevel > 0 ? 1 : 0;
                        //    item.show = true;
                        //    item.collapsed = false;
                        //}

                    } else {
                        $(e.target).removeClass('collapse');
                        $(e.target).addClass('expand');
                        $.each($scope.CurrentSearchResult, function (k, v) {
                            v.indent = v.itemlevel > 0 ? 1 : 0;
                            v.show = v.itemlevel == 0;
                            v.collapsed = true;
                        });
                        //for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                        //    var item = igrid.grid.getDataItem(i);
                        //    item.indent = item.itemlevel > 0 ? 1 : 0;
                        //    item.show = item.itemlevel == 0;
                        //    item.collapsed = true;
                        //}
                    }
                    $timeout(function () {
                        $scope.igrid.UpdateData($scope.CurrentSearchResult);
                    }, 20);
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                }

            });

            $scope.igrid = igrid;
            $scope.igrid.filter = function (item, args) {
                if (!item.show) return false;

                for (var columnId in args.columnFilters) {
                    if (columnId !== undefined && args.columnFilters[columnId] !== "") {
                        var cindex = args.grid.getColumnIndex(columnId);
                        var c = args.grid.getColumns()[cindex];

                        if (typeof (c.commafilter) != 'undefined' && c.commafilter) {
                            if (item[c.field] == null)
                                return false;
                            var listofvalues = args.columnFilters[columnId].split(',');
                            var exist = false;
                            for (var lv = 0; !exist && lv < listofvalues.length; lv++) {
                                if (item[c.field].toString().toLowerCase().startsWith(listofvalues[lv].toLowerCase())) {
                                    exist = true;
                                }
                                if (listofvalues[lv].toString().toLowerCase().startsWith('%') &&
                                    item[c.field].toString().toLowerCase().indexOf(listofvalues[lv].toLowerCase().substr(1)) >= 0)
                                    exist = true;
                            }
                            if (!exist)
                                return false;
                        }
                        else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'number') {
                            if (args.columnFilters[columnId].length > 0) {
                                var operator = args.columnFilters[columnId].charAt(0);
                                if (operator == '>' || operator == '<' || operator == '=') {
                                    var d = parseFloat(args.columnFilters[columnId].substr(1));
                                    switch (args.columnFilters[columnId].charAt(0)) {
                                        case '>':
                                            return item[c.field] > d;
                                            break;
                                        case '<':
                                            return item[c.field] < d;
                                            break;
                                        case '=':
                                            return item[c.field] == d;
                                            break;
                                    }
                                }
                                else {
                                    var d = parseFloat(args.columnFilters[columnId]);
                                    if (typeof (item[c.field]) != 'undefined') {
                                        return item[c.field] == d;
                                    }
                                    var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                                    return fvalue == d;
                                }
                                //return item[c.field] != null && item[c.field].toString().indexOf(args.columnFilters[columnId]) > 0;
                            }
                        }
                        else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'reference') {
                            //if (item[c.field] == null)
                            //    return false;

                            var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                            if (typeof (fvalue) == 'undefined' || fvalue == null ||
                                !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                                if (!args.columnFilters[columnId].startsWith('%') ||
                                    fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                                    return false;
                                }
                            }
                        }
                        else {

                            var fvalue = typeof (item[c.field]) != 'undefined' ? item[c.field] : c.formatter(0, cindex, item[c.field], c, item);
                            if (fvalue == null ||
                                !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                                if (!args.columnFilters[columnId].startsWith('%') ||
                                    fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                }
                return true;
            }

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.cell == 0) {
                    var item = $scope.igrid.dataView.getItem(args.row);
                    if (item) {
                        item.collapsed = !item.collapsed;
                        $.each($scope.CurrentSearchResult, function (k, v) {
                            if (v.ledgerid == item.ledgerid && v.itemlevel != item.itemlevel) {
                                v.show = !item.collapsed;
                            }
                        });

                        $timeout(function () {
                            $scope.igrid.referesh();
                        }, 100);
                    }
                    e.stopImmediatePropagation();
                }
            });

            $.each(results, function (k, v) {
                v.indent = v.itemlevel > 0 ? 1 : 0;
                v.show = v.itemlevel == 0;
                v.collapsed = true;
            });
            $scope.CurrentSearchResult = results;
            $scope.igrid.UpdateData(results);
        }

        $scope.SearchTrailBalance = function () {

            if ($scope.SearchCondition.locationid == undefined || $scope.SearchCondition.locationid == null)
                $scope.SearchCondition.locationid = -1;

            var from = $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy');
            var to = $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');
            $scope.ReportList = restService.Get('VoucherService/TrialBalance/' + $scope.SearchCondition.companyid + '/' +
                 $scope.SearchCondition.locationid + '/' +
                from + '/' + to, true).then(function (results) {
                    $scope.ConfigGrid(results);
                });
        }



        //$timeout(function () {
        //    $scope.SearchTrailBalance();
        //}, 10);

    }]);


