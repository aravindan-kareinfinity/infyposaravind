var app = angular.module('InfinityLab');
app.controller('DayBookController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.ShowLedgerPrint = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = false;
        $scope.AllowEnter = false;
        $scope.SearchResult = [];
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
        });

        $scope.Ledgers = restService.Get('ReferencelistService/LEDGERHEADERS', true).then(function (results) {
            $scope.Ledgers = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
            //$rootScope.BindLocation($scope, 'SearchCondition.locationid');
        });

        $scope.ErrorStyle = "success";

        $scope.ErrorMessage = "";


        $scope.SearchDaybook = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "VoucherService/SearchDayBook",
                data: srequest
            }).success(function (result) {
                $scope.BuildGrid(result);
                if ($scope.SearchCondition.ledgerid &&
                    $scope.SearchCondition.ledgerid > 0) {
                    $scope.ShowLedgerConfig = btoa(JSON.stringify({
                        'c': $scope.SearchCondition.companyid,
                        'f': $scope.SearchCondition.fromdate,
                        't': $scope.SearchCondition.todate,
                        'ln': $scope.SearchCondition.locationid,
                        'ld': $scope.SearchCondition.ledgerid
                    }));
                    $scope.ShowLedgerPrint = true;
                }
            }).error(function (result) {
            });
        }

        $scope.PrintLedger = function () {
            $rootScope.Download('LedgerService/PrintLedgerPDF/' + $scope.UserInfo.OrganizationID
                + '/' + $scope.ShowLedgerConfig, 'debitnote.pdf', 'application/pdf');
        }

        $scope.refvouchertypelist = [
            { 'id': -1, 'typeid': 1, 'name': 'Payment' },
            { 'id': -2, 'typeid': 2, 'name': 'Receipt' },
            { 'id': -3, 'typeid': 3, 'name': 'Purchase' },
            { 'id': -4, 'typeid': 4, 'name': 'Sales' },
            { 'id': -5, 'typeid': 5, 'name': 'Credit Note' },
            { 'id': -6, 'typeid': 6, 'name': 'Debit Note' },
            { 'id': -7, 'typeid': 7, 'name': 'Contra' },
            { 'id': -8, 'typeid': 8, 'name': 'Journal Cr' },
            { 'id': -9, 'typeid': 9, 'name': 'Cash Deposit' },
            { 'id': -10, 'typeid': 10, 'name': 'Cash Widhrawal' },
            { 'id': -11, 'typeid': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': -12, 'typeid': 12, 'name': 'Petty Cash to Main Cash' },
            { 'id': -13, 'typeid': 13, 'name': 'Cash Counter Expense' },
            { 'id': -14, 'typeid': 14, 'name': 'Advance Payment' },
            { 'id': -15, 'typeid': 15, 'name': 'Petty Cash Expense' },
            { 'id': -16, 'typeid': 16, 'name': 'Journal Dr' },


            { 'id': -101, 'typeid': 4, 'name': 'Customer Sales' },
            { 'id': -102, 'typeid': 4, 'name': 'Cusotmer Sales Return' },
            { 'id': -103, 'typeid': 4, 'name': 'Counter Sales' },
            { 'id': -104, 'typeid': 4, 'name': 'Counter Sales Return' },
            { 'id': -105, 'typeid': 3, 'name': 'Purchase Inventory' },
            { 'id': -106, 'typeid': 3, 'name': 'Purchase Return' },
            { 'id': -107, 'typeid': 2, 'name': 'Credit Payment' },
            { 'id': -108, 'typeid': 1, 'name': 'Supplier Payment' },
        ];

        restService.Get('ReferencelistService/FULL-VOUCHERTYPE', true).then(function (results) {
            var newvoucherList = (JSON.parse(JSON.stringify($scope.refvouchertypelist)));
            $.each(results, function (k, v) {
                newvoucherList.push({ 'id': v.id, 'name': v.name, 'typeid': v.additionalinfo.parentid });
            })
            $scope.refvouchertypelist = newvoucherList;
        });


        $scope.igrid = null;
        $scope.BuildGrid = function (result) {
            $scope.ShowClosingBalance = false;
            if (result && result.length > 0 && result[result.length - 1].id == -2) {
                $scope.ShowClosingBalance = true;
                $scope.ClosingBalance = result[result.length - 1];
                result.splice(result.indexOf($scope.ClosingBalance), 1);
                $scope.ClosingBalance.credit = $scope.ClosingBalance.crdr == 'Cr' ? $filter('IndianNumber')($scope.ClosingBalance.amount, 2) + 'Cr' : '';
                $scope.ClosingBalance.debit = $scope.ClosingBalance.crdr == 'Dr' ? $filter('IndianNumber')($scope.ClosingBalance.amount, 2) + 'Dr' : '';
            }
            if ($scope.igrid != null)
                $scope.igrid.destroy();


            var newlist = [];
            $.each(result, function (k, v) {
                if (v.isheader) {
                    if (newlist.length > 0) {
                        var parant = newlist[0];
                        $.each(newlist, function (kk, vv) {
                            vv.grps = newlist;
                            vv.prnt = parant;
                        })
                    }
                    newlist = [];
                    newlist.push(v);
                }
                else {
                    newlist.push(v);
                }
            })
            if (newlist.length > 0) {
                var parant = newlist[0];
                $.each(newlist, function (kk, vv) {
                    vv.grps = newlist;
                    vv.prnt = parant;
                })
            }

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddFormatrerColumn('grouping', "<span class='infygridtoggle expand'></span>", 40, function (row, cell, value, columnDef, dataContext) {
                var spacer = "<span style='display:inline-block;height:1px;width:8px'></span>";
                if (dataContext.isheader) {
                    if (dataContext.collapsed) {
                        return spacer + " <span class='infygridtoggle expand'></span>&nbsp;";
                    } else {
                        return spacer + " <span class='infygridtoggle collapse'></span>&nbsp;";
                    }
                } else {
                    return spacer + " <span class='infygridtoggle'></span>&nbsp;";
                }
            }).sortable = false;

            var column = igrid.AddCustomColumn('vouchertype', 'Voucher', 100, function (item, col) {
                if (item.isheader)
                    return $scope.SearchRef(item.vouchertype, $scope.refvouchertypelist, '');
                return '';
            });

            var column = igrid.AddCustomColumn('Date', 'Date', 100, function (item, col) {
                if (item.isheader) return $filter('date')(item.date, 'dd-MM-yyyy');
                return '';
            });

            var column = igrid.AddCustomColumn('NO', 'Voucher No', 100, function (item, col) {
                if (item.isheader) return item.no;
                return '';
            });

            igrid.AddSimpleColumn('ledger', 'Particular', 350);

            var column = igrid.AddCustomColumn('Debit', 'Debit', 100, function (item, col) {
                if (item.id == -2) return '';
                //if (item.collapsed && item.show && item.isheader) {
                //    var dr = 0;
                //    $.each(item.grps, function (k, v) {
                //        if (v.crdr == 'Dr')
                //            dr += v.amount;
                //    })
                //    return $filter('IndianNumber')(dr, 2);
                //}
                //else
                {
                    if (item.crdr == 'Dr') return $filter('IndianNumber')(item.amount, 2);
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.id == -2) return 0;
                //if (item.collapsed && item.show && item.isheader) {
                //    var dr = 0;
                //    $.each(item.grps, function (k, v) {
                //        if (v.crdr == 'Dr')
                //            dr += v.amount;
                //    })
                //    return dr;
                //}
                if (item.crdr == 'Dr') return item.amount;
                return 0;
            }

            var column = igrid.AddCustomColumn('Credit', 'Credit', 100, function (item, col) {
                //if (item.collapsed && item.show && item.isheader) {
                //    var cr = 0;
                //    $.each(item.grps, function (k, v) {
                //        if (v.crdr == 'Cr')
                //            cr += v.amount;
                //    })
                //    return $filter('IndianNumber')(cr, 2);
                //}
                //else
                {
                    if (item.crdr == 'Cr') return $filter('IndianNumber')(item.amount, 2);
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.id == -2) return 0;
                //if (item.collapsed && item.show && item.isheader) {
                //    var cr = 0;
                //    $.each(item.grps, function (k, v) {
                //        if (v.crdr == 'Cr')
                //            cr += v.amount;
                //    })
                //    return cr;
                //}
                if (item.crdr == 'Cr') return item.amount;
                return 0;
            }

            $scope.igrid = igrid;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.CurrentSearchResult = result;
            $.each(result, function (k, v) {
                v.show = v.isheader;
                v.collapsed = true;
            });

            

            igrid.grid.onDblClick.subscribe(function (e, args) {
                var columnid = args.grid.getColumns()[args.cell].id;
                var item = args.grid.getDataItem(args.row);
                if (columnid == 'vouchertype' && item.uid && item.uid.startsWith('V_')) {
                    $timeout(function () {
                        $location.url('/voucher?id=' + item.id);
                    }, 10);
                }
            });

            igrid.grid.onHeaderClick.subscribe(function (e, args) {
                if (args.column.id == "grouping") {
                    var columnElement = e.target;
                    if ((e.target).className.indexOf('expand') > 0) {
                        $(columnElement).removeClass("expand");
                        $(columnElement).addClass("collapse");
                        $.each($scope.CurrentSearchResult, function (k, v) {
                            v.show = true;
                            v.collapsed = false;
                        });
                    }
                    else {
                        $(columnElement).addClass("expand");
                        $(columnElement).removeClass("collapse");
                        $.each($scope.CurrentSearchResult, function (k, v) {
                            v.show = v.isheader;
                            v.collapsed = true;
                        });
                    }
                    e.stopImmediatePropagation();
                    args.grid.getData().refresh();
                    args.grid.invalidate();
                    args.grid.render();
                }
            });

            $scope.igrid.filter = function (item, args) {
                if (!item.show)
                    return false;
                var count = 0;
                for (var columnId in args.columnFilters) {
                    count++;
                }
                if (count == 0) return item.show;
                var hassuccess = false;
                for (var i = 0; i < item.grps.length; i++) {
                    if ($scope.FilterGroupLevel(item.grps[i], args)) {
                        hassuccess = true;
                        break;
                    }
                }
                return hassuccess;
            }

            $scope.FilterGroupLevel = function (item, args) {
                var result = false;
                for (var columnId in args.columnFilters) {
                    if (columnId !== undefined && args.columnFilters[columnId] !== "") {
                        var cindex = args.grid.getColumnIndex(columnId);
                        var c = args.grid.getColumns()[cindex];
                        if (item[c.field] == undefined || item[c.field] == null) {
                            result = false;
                            break;
                        }
                        if (typeof (c.commafilter) != 'undefined' && c.commafilter) {
                            if (item[c.field] == null) {
                                result = false;
                                break;
                            }
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
                            if (!exist) {
                                result = false;
                                break;
                            }
                            else {
                                result = true;
                                break;
                            }
                        }
                        else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'number') {
                            if (args.columnFilters[columnId].length > 0) {
                                var operator = args.columnFilters[columnId].charAt(0);
                                if (operator == '>' || operator == '<' || operator == '=') {
                                    var d = parseFloat(args.columnFilters[columnId].substr(1));
                                    switch (args.columnFilters[columnId].charAt(0)) {
                                        case '>':
                                            result = item[c.field] > d;
                                            break;
                                        case '<':
                                            result = item[c.field] < d;
                                            break;
                                        case '=':
                                            result = item[c.field] == d;
                                            break;
                                    }
                                }
                                else {
                                    var d = parseFloat(args.columnFilters[columnId]);
                                    if (typeof (item[c.field]) != 'undefined') {
                                        result = item[c.field] == d;
                                    }
                                    var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                                    result = fvalue == d;
                                }
                                if (result != null)
                                    break;
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
                                    result = false;
                                    break;
                                }
                                else {
                                    result = true;
                                }
                            }
                        }
                        else {

                            var fvalue = typeof (item[c.field]) != 'undefined' ? item[c.field] : c.formatter(0, cindex, item[c.field], c, item);
                            if (fvalue == null ||
                                !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                                if (!args.columnFilters[columnId].startsWith('%') ||
                                    fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                                    result = false;
                                    break;
                                }
                            }
                            result = true;
                            break;
                        }
                    }
                }
                if (result == null)
                    return true;
                return result;
            }

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.cell == 0) {
                    var item = $scope.igrid.dataView.getItem(args.row);
                    if (item) {
                        item.collapsed = !item.collapsed;
                        $.each($scope.CurrentSearchResult, function (k, v) {
                            if (v.uid == item.uid && !v.isheader) {
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

            $scope.igrid.UpdateData(result, '_id');
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle, true);
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

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
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
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "ShopactivityService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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

        }

    }]);


