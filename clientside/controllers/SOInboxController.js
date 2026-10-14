var app = angular.module('InfinityLab');
app.controller('SOInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    'ngDialog', '$state', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Suppliers = [];
        $scope.Company = [];
        $scope.Location = [];
        $scope.ismfgapplication = false;
        $scope.Customers = restService.Get('ReferencelistService/LIST-CUSTOMER', true).then(function (results) {
            $scope.Customers = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'All Locations' });
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/LIST-PRODUCT', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });

        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });

        $scope.SearchRequest = {
            searchtype: 0,
            locationid: 0,
            statusid: 1
        };

        $scope.SearchType = [
            { 'id': 0, 'name': 'Default' },
            { 'id': 1, 'name': 'Customer' },
            { 'id': 2, 'name': 'Agent' },
            { 'id': 3, 'name': 'City' },
            { 'id': 4, 'name': 'Product' },
            { 'id': 5, 'name': 'Design' }];

        $scope.StatusList = [{ 'id': 0, 'name': 'All' },
        { 'id': 1, 'name': 'Pending' },
        { 'id': 2, 'name': 'Planned' },
        { 'id': 3, 'name': 'Cancelled' },
        { 'id': 4, 'name': 'Billed' },
        { 'id': 6, 'name': 'Removed' },
        { 'id': 5, 'name': 'Planning' }];

        $scope.SOList = [];



        $scope.ShowSizeList = function () {
            var request = {
                "item": $scope.SearchRequest,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalesorderService/SearchSizeSummary",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $scope.SizeGroupResult = result;
                $scope.Mode = 'SizeGroup';
                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
                igrid.options.showHeaderRow = true;

                igrid.AddSimpleColumn('sono', 'Order No', 100);
                if ($scope.mfg_customerpo)
                    igrid.AddSimpleColumn('customerpo', 'PO No', 80);
                igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('customername', 'Customer', ($scope.Mode == 'LIST') ? 150 : 300);
                igrid.AddSimpleColumn('agentname', 'Agent', 150);
                igrid.AddSimpleColumn('cityname', 'City', 100);
                igrid.AddSimpleColumn('productname', 'Product', 150);
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.sizegroup)
                    igrid.AddSimpleColumn('sizegroup', 'Size/Set', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.brand)
                    igrid.AddSimpleColumn('brandname', 'Brand', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.style)
                    igrid.AddSimpleColumn('stylename', 'Style', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.design)
                    igrid.AddSimpleColumn('designid', 'Design', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.colour)
                    igrid.AddSimpleColumn('colourname', 'Colour', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.sleeve)
                    igrid.AddSimpleColumn('sleevename', 'Sleeve', 100).commafilter = true;
                igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;

                $.each(result.sizelist, function (k, v) {
                    var column = igrid.AddCustomColumn('size_' + k, v, 40,
                        function (item, col) {
                            if (item.sz[col.Index] > 0)
                                return item.sz[col.Index];
                            return '';
                        });

                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.sz[col.Index] > 0)
                            return item.sz[col.Index];
                        return 0;
                    }
                });

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result.items, '_id');

            }).error(function (result) {
            });
        }


        $scope.SearchSOSummary = function () {
            $scope.totalrecords = 0;
            $scope.SOList = [];

            request = {
                "item": $scope.SearchRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: $scope.mfg_mode == 6 ? "SalesorderService/SearchSizeSummary" : "SalesorderService/SearchUIDSummary",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SOList = result;
                $scope.totalrecords = result.length;
                //$scope.ShowList(false);
                $scope.ShowUIDList(false, true);
            }).error(function (result) {
            });
        }

        $scope.Print = function () {

            var rows = this.igrid.GetSelectedRows();
            if (rows.length == 0) {
                rows = $scope.SOList;
            }
            var printrows = [];
            $.each(rows, function (k, v) {
                if (v.plannedqty > 0) {
                    v.cityname = $scope.SearchRef(v.cityid, $scope.CityList, '');
                    v.productname = $scope.SearchRef(v.productid, $scope.ProductList, '');
                    var customer = $scope.SearchRef(v.customerid, $scope.Customers, '');
                    if (v.shipto && v.shipto > 0) {
                        customer += ' - ' + $scope.SearchRef(v.shipto, $scope.Customers, '');
                    }
                    v.customername = customer;
                    printrows.push(v);
                }
            });

            var sortBy = [{
                prop: 'customername',
                direction: 1
            },
            {
                prop: 'sono',
                direction: 1
            }

                , {
                prop: 'productname',
                direction: 1
            }, {
                prop: 'designitemcode',
                direction: 1
            }];

            printrows.sort(function (a, b) {
                var i = 0, result = 0;
                while (i < sortBy.length && result === 0) {
                    result = sortBy[i].direction * (a[sortBy[i].prop].toString() < b[sortBy[i].prop].toString() ? -1 : (a[sortBy[i].prop].toString() > b[sortBy[i].prop].toString() ? 1 : 0));
                    i++;
                }
                return result;
            });

            var list = [];

            list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
            list.push($rootScope.PrintFormater.Bold("Delivery Plan ") + $filter('date')(new Date(), 'dd-MM-yyyy'));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            var customername = ''
            var cityname = '';
            var customerid = 0;
            $.each(printrows, function (k, v) {
                if (v.cityname != cityname) {
                    if (cityname != '') {
                        list.push("\r\n");
                        list.push($rootScope.PrintFormater.Cutter());
                    }
                    cityname = v.cityname;
                    list.push($rootScope.PrintFormater.Bold(cityname));
                }
                if (v.customername != customername) {
                    if (customerid > 0 && customerid == v.customerid) {
                        list.push($rootScope.PrintFormater.Barcode('SOPLAN/' + customerid, true));
                    }
                    customername = v.customername;
                    customerid = v.customerid;
                    list.push($rootScope.PrintFormater.Bold(customername));
                }
                var clist = [];
                clist.push($rootScope.PrintFormater.Normal(v.productname, 10, false));
                clist.push($rootScope.PrintFormater.Normal(v.designid, 8));
                clist.push($rootScope.PrintFormater.Normal(v.sizegroup, 8));
                clist.push($rootScope.PrintFormater.Normal(v.colourname, 8));
                clist.push($rootScope.PrintFormater.Align(v.plannedqty, 4, true));
                list.push(clist.join(' '));
            })

            var barcodevalue = $rootScope.PrintFormater.Barcode('SOPLAN/' + customerid);
            var base64 = function (s) { return $window.btoa(unescape(encodeURIComponent(s))); }
            var data = base64(barcodevalue);
            list.push($rootScope.PrintFormater.Barcode('SOPLAN/' + customerid));

            list.push("\r\n");
            list.push("\r\n");
            list.push($rootScope.PrintFormater.Cutter());
            list.push("\r\n");
            list.push("\r\n");

            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }


        $scope.ShowFilterList = function () {
            var rows = [];
            var grows = this.igrid.GetSelectedRows();
            $.each(grows, function (k, v) {
                var pqty = 0;
                $.each(v.list, function (kk, vv) {
                    rows.push(vv);
                });
                v.plannedqty = pqty;
            });
            $scope.Mode = 'LIST';
            $scope.SOConfig(rows);
        }

        $scope.PlanAvailable = function () {

            var rows = [];
            var grows = this.igrid.GetSelectedRows();
            $.each(grows, function (k, v) {
                $.each(v.list, function (kk, vv) {
                    rows.push(vv);
                });
            });

            if (rows.length == 0) return;
            $scope.FixedPlan(rows);
        }


        $scope.DesignUIDSummary = function () {
            $scope.Mode = 'SUMMARY';
            var summary = [];
            var summaryKey = [];
            $.each($scope.SOList, function (k, v) {
                var key = v.designitemcode + v.productid;
                if (typeof (summaryKey[key]) == 'undefined') {
                    var citem = (JSON.parse(JSON.stringify(v)));
                    citem.list = [];
                    if (citem.availableqty > citem.pendingqty)
                        citem.availableqty = citem.pendingqty;
                    citem.stockqty = 0;
                    $.each(citem.sizelist, function (kk, vv) {
                        citem.stockqty += (vv.st && vv.st > 0) ? vv.st : 0;
                    })
                    summary.push(citem);
                    summaryKey[key] = citem;
                }
                else {
                    summaryKey[key].plannedqty += v.plannedqty;
                    summaryKey[key].pendingqty += v.pendingqty;
                    summaryKey[key].billedqty += v.billedqty;
                    summaryKey[key].availableqty += (v.availableqty > v.pendingqty ? v.pendingqty : v.availableqty);
                    summaryKey[key].allocatedqty += v.allocatedqty;
                    summaryKey[key].delivery += v.delivery;
                    $.each(v.sizelist, function (kk, vv) {
                        var found = summaryKey[key].sizelist.find(function (sitem) {
                            return sitem.id == vv.id;
                        });
                        if (found && found != null) {
                            found.q += vv.q;
                        }
                        else {
                            summaryKey[key].sizelist.push({ 'id': vv.id, 'q': vv.q, 'st': 0 });
                        }
                    })
                }
                summaryKey[key].list.push(v);
            });
            $.each(summary, function (k, v) {
                if (v.delivery > 0)
                    v.delivery = (v.delivery / v.list.length);
                v.style = null;
                if ($scope.SearchRequest.statusid == 5) {
                    v.excessstock = 0;
                    v.excessorder = 0;
                    $.each(v.sizelist, function (kk, vv) {
                        if (vv.q > vv.st) {
                            v.excessorder += (vv.q - vv.st);
                        }
                        if (vv.st > vv.q) {
                            v.excessstock += (vv.st - vv.q);
                        }
                    });
                }
            });
            $scope.DesignSummaryList = summary;

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('productid', 'Product', 150, $scope.ProductList);
            igrid.AddSimpleColumn('designitemcode', 'Design', 150).commafilter = true;
            igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('availableqty', 'Available', 80, 'N0', 2).total = false;
            //igrid.AddFormatedColumn('allocatedqty', 'Allocated', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('pipelinestock', 'WIP', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('delivery', 'D.P %', 100, 'N2', 2).total = false;
            if ($scope.SearchRequest.statusid == 5) {
                igrid.AddFormatedColumn('excessstock', 'Stock ++', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('excessorder', 'Order ++', 80, 'N0', 2).total = true;
            }
            else {
                igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;

            }
            var slist = $scope.system_possizelist.split(',');
            $.each(slist, function (k, v) {
                var column = igrid.AddCustomColumn('size_' + k, v, 40, function (item, col) {
                    if (item.sizelist && item.sizelist.length > 0) {
                        var qty = 0;
                        var sizeqty = item.sizelist.find(function (sitem) {
                            return sitem.id == col.size;
                        })

                        if (sizeqty && sizeqty.q && sizeqty.q > 0)
                            return $filter('IndianNumber')(sizeqty.q, 0)
                    }
                    return '';
                });
                column.cssClass = 'rightalign';
                column.size = v;
                column.sortable = true;
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.sizelist && item.sizelist.length > 0) {
                        var qty = 0;
                        var sizeqty = item.sizelist.find(function (sitem) {
                            return sitem.id == col.size;
                        })

                        if (sizeqty && sizeqty.q && sizeqty.q > 0)
                            return sizeqty.q;
                    }
                    return 0;
                }
            })
            if ($scope.SearchRequest.statusid == 5) {

                igrid.AddFormatedColumn('stockqty', 'Stock', 80, 'N0', 2).total = true;
                $.each(slist, function (k, v) {
                    var column = igrid.AddCustomColumn('stock_' + k, v, 40, function (item, col) {
                        if (item.sizelist && item.sizelist.length > 0) {
                            var qty = 0;
                            var sizeqty = item.sizelist.find(function (sitem) {
                                return sitem.id == col.size;
                            })

                            if (sizeqty && sizeqty.st && sizeqty.st > 0)
                                return $filter('IndianNumber')(sizeqty.st, 0)
                        }
                        return '';
                    });
                    column.cssClass = 'rightalign';
                    column.size = v;
                    column.sortable = true;
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.sizelist && item.sizelist.length > 0) {
                            var qty = 0;
                            var sizeqty = item.sizelist.find(function (sitem) {
                                return sitem.id == col.size;
                            })

                            if (sizeqty && sizeqty.st && sizeqty.st > 0)
                                return sizeqty.st;
                        }
                        return 0;
                    }
                })
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.DesignSummaryList, '_id');
        }

        $scope.SOUIDSummary = function () {
            $scope.Mode = 'SUMMARY';
            var summary = [];
            var summaryKey = [];
            $.each($scope.SOList, function (k, v) {
                var key = v.sono + '-' + v.soid + v.cid;
                if (typeof (summaryKey[key]) == 'undefined') {
                    var citem = (JSON.parse(JSON.stringify(v)));
                    citem.list = [];
                    if (citem.availableqty > citem.pendingqty)
                        citem.availableqty = citem.pendingqty;
                    summary.push(citem);
                    summaryKey[key] = citem;
                }
                else {
                    summaryKey[key].oqty += v.oqty;
                    summaryKey[key].plannedqty += v.plannedqty;
                    summaryKey[key].pendingqty += v.pendingqty;
                    summaryKey[key].billedqty += v.billedqty;
                    summaryKey[key].availableqty += (v.availableqty > v.pendingqty ? v.pendingqty : v.availableqty);
                    summaryKey[key].allocatedqty += v.allocatedqty;
                    summaryKey[key].delivery += v.delivery;
                }
                summaryKey[key].list.push(v);
            });
            $.each(summary, function (k, v) {
                if (v.delivery > 0)
                    v.delivery = (v.delivery / v.list.length);
                v.style = null;
            });
            $scope.DesignSummaryList = summary;

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddButtonColumn('PDF', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='PDF' class='btn btn-success btn-xs'><i class='fa fa-file-pdf-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Slip', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='PDF' class='btn btn-primary btn-xs'><i class='fa fa-file-pdf-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");


            igrid.AddSimpleColumn('sono', 'Order No', 100);
            if ($scope.mfg_customerpo)
                igrid.AddSimpleColumn('customerpo', 'PO No', 80);
            igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');
            var column = igrid.AddCustomColumn('customer', 'Customer', 150, function (item, col) {
                var customer = $scope.SearchRef(item.customerid, $scope.Customers, '');
                if (item.shipto && item.shipto > 0) {
                    customer += ' - ' + $scope.SearchRef(item.shipto, $scope.Customers, '');
                }
                return customer;
            });
            column.sortable = true;
            column.filtertype = 'reference';
            column.enablefilter = true;
            column.getValue = function (item, col) {
                var customer = $scope.SearchRef(item.customerid, $scope.Customers, '');
                if (item.shipto && item.shipto > 0) {
                    customer += ' - ' + $scope.SearchRef(item.shipto, $scope.Customers, '');
                }
                return customer;
            }

            igrid.AddSimpleColumn('remark', 'Remark', 80);
            igrid.AddSimpleColumn('agentname', 'Agent', 100);
            igrid.AddReferenceColumn('salesmenid', 'Salesmen', 100, $scope.Employees);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.CityList);
            igrid.AddReferenceColumn('createdby', 'Created', 100, $scope.Employees);
            igrid.AddFormatedColumn('oqty', 'Ordered', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;
            var column = igrid.AddCustomColumn('st', 'Status', 60, function (item, col) {
                return item.sostatus == 'C' ? 'Completed' : (item.sostatus == 'P' ? 'Partial' : 'New');
            });
            column.sortable = true;
            column.enablefilter = true;
            column.filtertype = 'reference';
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;

                if (command == 'Slip') {
                    $scope.$apply(function () {
                        $rootScope.Download('SalesorderService/PrintSLIP/' + $scope.UserInfo.OrganizationID + '/' + item.soid, 'salesorder.pdf', 'application/pdf');
                    })
                }

                if (command == 'PDF') {
                    $scope.$apply(function () {
                        $rootScope.Download('SalesorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.soid, 'salesorder.pdf', 'application/pdf');
                    })
                }
            });
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.DesignSummaryList, '_id');
            $scope.summaryselectedcount = 0;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.summaryselectedcount = range.length;
                }, 10);
            };
        }


        $scope.AssignPONumber = function () {
            var items = $scope.igrid.GetSelectedRows();
            if (items.length == 0)
                return;

            var dialog = ngDialog.open({
                template: 'AssignPODialog',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.Save = function () {
                        if ($scope.customerpo && $scope.customerpo != null &&
                            $scope.customerpo != '') {
                            $scope.closeThisDialog($scope.customerpo);
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": {
                                'ids': [],
                                'customerpo': data.value
                            },
                            "securitytoken": "String content"
                        }

                        $.each(items, function (k, v) { request.Item.ids.push(v.soid) });
                   
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "Salesorderservice/AssignPO",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                $.each(items, function (k, v) {
                                    v.customerpo = data.value;
                                });
                            }
                        }).error(function (result) {
                        });

                    }
                }
            });
        }


        $scope.DesignSummaryList = null;
        $scope.DesignSummary = function () {
            var summary = [];
            var summaryKey = [];
            $.each($scope.SOList, function (k, v) {
                var key = v.designid + '-' + v.sizeset + '-' + v.colourid + '-' + v.productid;
                if (typeof (summaryKey[key]) == 'undefined') {
                    var citem = (JSON.parse(JSON.stringify(v)));
                    citem.list = [];
                    if (citem.availableqty > citem.pendingqty)
                        citem.availableqty = citem.pendingqty;
                    summary.push(citem);
                    summaryKey[key] = citem;
                }
                else {
                    summaryKey[key].plannedqty += v.plannedqty;
                    summaryKey[key].pendingqty += v.pendingqty;
                    summaryKey[key].billedqty += v.billedqty;
                    summaryKey[key].availableqty += (v.availableqty > v.pendingqty ? v.pendingqty : v.availableqty);
                    summaryKey[key].allocatedqty += v.allocatedqty;
                    summaryKey[key].delivery += v.delivery;
                }
                summaryKey[key].list.push(v);
            });
            $.each(summary, function (k, v) {
                if (v.delivery > 0)
                    v.delivery = (v.delivery / v.list.length);
            });
            $scope.DesignSummaryList = summary;

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('productname', 'Product', 150);
            igrid.AddSimpleColumn('designid', 'Design', 100).commafilter = true;
            igrid.AddSimpleColumn('colourname', 'Colour', 100).commafilter = true;
            igrid.AddSimpleColumn('sizegroup', 'Size/Set', 100).commafilter = true;
            igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('availableqty', 'Available', 80, 'N0', 2).total = false;
            igrid.AddFormatedColumn('allocatedqty', 'Allocated', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('pipelinestock', 'WIP', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('delivery', 'D.P %', 100, 'N2', 2).total = false;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.DesignSummaryList, '_id');
        }

        $scope.Mode = 'LIST';

        $scope.DownloadSummaryPDF = function () {
            var seletedRows = $scope.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.soid);
            })
            $rootScope.PostDownload("SalesorderService/PrintSLIP", {
                'Item': {
                    'ids':ids
                }
            }, 'PrintSLIP.pdf', 'application/pdf');
        }

        $scope.ShowUIDList = function (summary, process) {

            $scope.DesignSummaryList = null;
            $scope.Mode = summary ? 'SUMMARY' : 'LIST';
            if ($scope.mfg_mode == 6) {
                $scope.SOSizeConfig();
                return;
            }
            if (process) {
                var propertylist = 'sd=sodate,sz=szlist,rm=remark,aid=agentid,zt=stlist,cyid=cityid,sb=sobookno,cbn=customerpo,sid=shipto,d=designitemcode,p=pendingqty,o=orderqty,n=plannedqty,v=availableqty,l=allocatedqty,cid=customerid,b=billedqty,c=cancelledqty,r=removedqty,pi=pipelinestock,ps=partialstock,cb=createdby,st=sostatus,slm=salesmenid'.split(',');
                var attriblist = [];
                $.each(propertylist, function (k, v) {
                    attriblist.push({ 'key': v.split('=')[1], 'value': v.split('=')[0] });
                })
                var sizelist = [];
                $.each($scope.SizeList, function (k, v) {
                    sizelist[v.id] = v.name;
                })

                $.each($scope.SOList, function (k, v) {
                    $.each(attriblist, function (kk, vv) {
                        v[vv.key] = v[vv.value];
                        delete v[vv.value];
                    });
                    v.sizelist = [];
                    v.oqty = 0;
                    v.eqty = 0;
                    var sids = v.ids.split(',');
                    $.each(v.szlist.split(','), function (kk, vv) {
                        var data = vv.split('~');
                        v.oqty += parseInt(data[1]);
                        if (parseInt(data[1]) < parseInt(data[5]))
                            v.eqty += (parseInt(data[5]) - parseInt(data[1]));

                        var pqty = parseInt(data[1]) - (parseInt(data[4]) + parseInt(data[5]) + parseInt(data[3]));
                        if (pqty < 0) pqty = 0;
                        var sizeid = sizelist[parseInt(data[0])];
                        var esize = v.sizelist.find(function (item) {
                            return item.id == sizeid;
                        });
                        if (esize && esize != null) {
                            esize.q += pqty;
                            esize.oq += parseInt(data[1]);
                            esize.op += parseInt(data[2]);
                            esize.or += parseInt(data[3]);
                            esize.oc += parseInt(data[4]);
                            esize.ob += parseInt(data[5]);
                            esize.sid += ',' + sids[kk];
                        }
                        else {
                            v.sizelist.push({
                                'id': sizelist[parseInt(data[0])],
                                'i': parseInt(data[0]), 'q': pqty,
                                'sid': sids[kk], 'op': parseInt(data[2]),
                                'or': parseInt(data[3]), 'oc': parseInt(data[4]),
                                'ob': parseInt(data[5]), 'oq': parseInt(data[1])
                            });
                        }
                    });
                    if (v.stlist && v.stlist != null) {
                        //v.stocklist = [];
                        v.possibleqty = 0;
                        v.plannedqty = 0;
                        $.each(v.stlist.split(','), function (kk, vv) {
                            var data = vv.split('-');
                            var sizeid = sizelist[parseInt(data[0])];
                            var esize = v.sizelist.find(function (item) {
                                return item.id == sizeid;
                            });
                            if (esize && esize != null) {
                                esize.pl = parseInt(data[2]);
                                esize.st = parseInt(data[1]);
                                if (isNaN(esize.pl)) esize.pl = 0;
                                if (isNaN(esize.st)) esize.st = 0;
                                v.plannedqty += esize.op;
                                var pendingqty = esize.q - esize.op;
                                if (pendingqty > 0 && (esize.st - esize.pl) > 0) {
                                    var possibleqty = 0;
                                    if ((esize.st - esize.pl) > pendingqty) {
                                        possibleqty = pendingqty;
                                    }
                                    else if ((esize.st - esize.pl) < pendingqty) {
                                        possibleqty = pendingqty - (esize.st - esize.pl);
                                    }
                                    v.possibleqty += possibleqty;
                                    esize.ps = possibleqty;
                                }
                                else {
                                    esize.ps = 0;
                                }
                                //v.stocklist.push({ 'id': sizelist[parseInt(data[0])], 'q': parseInt(data[1]) , 'p': possibleqty});
                            }
                        });
                    }
                    //v.oqty = v.pendingqty + v.billedqty + v.cancelledqty + v.removedqty;
                    v.delivery = 0;
                    if (v.availableqty > 0) {
                        if (v.availableqty >= v.pendingqty) {
                            v.delivery = 100;
                        }
                        else {
                            v.delivery = v.availableqty / v.pendingqty * 100;
                        }
                    }
                });


                if (summary) {
                    var summary = [];
                    var summaryKey = [];
                    $.each($scope.SOList, function (k, v) {
                        if (typeof (summaryKey[v.soid]) == 'undefined') {
                            var citem = (JSON.parse(JSON.stringify(v)));
                            citem.list = [];
                            if (citem.availableqty > citem.pendingqty)
                                citem.availableqty = citem.pendingqty;
                            summary.push(citem);
                            summaryKey[citem.soid] = citem;
                        }
                        else {
                            summaryKey[v.soid].plannedqty += v.plannedqty;
                            summaryKey[v.soid].pendingqty += v.pendingqty;
                            summaryKey[v.soid].availableqty += (v.availableqty > v.pendingqty ? v.pendingqty : v.availableqty);
                            summaryKey[v.soid].allocatedqty += v.allocatedqty;
                            summaryKey[v.soid].delivery += v.delivery;
                        }
                        summaryKey[v.soid].list.push(v);
                    });
                    $.each(summary, function (k, v) {
                        if (v.delivery > 0)
                            v.delivery = (v.delivery / v.list.length);
                    });
                    $scope.SOSummary = summary;
                }
            }
            $scope.SOUIDConfig();
        }

        $scope.SOSizeConfig = function (filterlist) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sono', 'Order No', 100);
            if ($scope.mfg_customerpo)
            igrid.AddSimpleColumn('customerpo', 'PO No', 80);
            igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('customername', 'Customer', 100);
            igrid.AddSimpleColumn('agentname', 'Agent', 100);
            igrid.AddSimpleColumn('salesmen', 'Salesmen', 100);
            igrid.AddSimpleColumn('cityname', 'City', 100);
            igrid.AddSimpleColumn('productname', 'Product', 100);
            igrid.AddSimpleColumn('sizegroup', 'Group', 100);
            igrid.AddSimpleColumn('designid	', 'Design', 100);
            igrid.AddFormatedColumn('orderqty', 'Ordered', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;

            var column = igrid.AddCustomColumn('st', 'Status', 40, function (item, col) {
                return item.sostatus == 'C' ? 'Completed' : (item.sostatus == 'P' ? 'Partial' : 'New');
            });
            column.sortable = true;
            column.enablefilter = true;
            column.filtertype = 'reference';

            igrid.AddReferenceColumn('createdby', 'Created', 100, $scope.Employees);
            igrid.AddSimpleColumn('remark', 'Remark', 80);
            igrid.AddSimpleColumn('sobookno', 'Book No', 80);
            if ($scope.mfg_customerpo)
            igrid.AddSimpleColumn('customerpo', 'PO No', 80);
            



            igrid.EnableTotal();
            igrid.Initialize();
            if ($scope.SearchRequest.statusid == 5) {
                igrid.setCellCSS('igridbigrows');
                igrid.dataView.getItemMetadata = function (row) {
                    var rowdata = igrid.grid.getData().getItem(row);

                    if (rowdata.plannedqty && rowdata.plannedqty > 0) {
                        if (rowdata.pendingqty == rowdata.plannedqty)
                            rowdata.style = 'igridbigrowswithgreen';
                        else
                            rowdata.style = 'igridbigrowswithdarkgreen';
                    } else if (rowdata.possibleqty && rowdata.possibleqty > 0) {
                        if (rowdata.possibleqty == rowdata.pendingqty)
                            rowdata.style = 'igridbigrows';
                        else
                            rowdata.style = 'igridbigrowswithmark';
                    }
                    else {
                        rowdata.style = 'igridbigrowswithred';
                    }
                    return { 'cssClasses': 'igridbigrows' };
                }
            }

            $scope.igrid = igrid;
            if ($scope.Mode == 'LIST')
                $scope.igrid.UpdateData($scope.SOList.items, '_id');
            else
                $scope.igrid.UpdateData($scope.SOSummary, '_id');

            igrid.grid.onDblClick.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'sono') {
                    var selectedIndexes = [];
                    for (var i = 0; i < args.grid.getDataLength(); i++) {
                        if (args.grid.getDataItem(i).sono == item.sono) {
                            if (item.style && args.grid.getDataItem(i).style) {
                                if (item.style == args.grid.getDataItem(i).style)
                                    selectedIndexes.push(i);
                            }
                            else {
                                selectedIndexes.push(i);
                            }
                        }
                    }
                    args.grid.setSelectedRows(selectedIndexes);
                    $timeout(function () {
                        args.grid.invalidateRows(selectedIndexes);
                        args.grid.render();
                    }, 10);
                }
            });
        }

        $scope.SOUIDConfig = function (filterlist) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sono', 'Order No', 100);
            if ($scope.mfg_customerpo)
            igrid.AddSimpleColumn('customerpo', 'PO No', 80);
            igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');

            var column = igrid.AddCustomColumn('customer', 'Customer', 150, function (item, col) {
                var customer = $scope.SearchRef(item.customerid, $scope.Customers, '');
                if (item.shipto && item.shipto > 0) {
                    customer += ' - ' + $scope.SearchRef(item.shipto, $scope.Customers, '');
                }
                return customer;
            });
            column.sortable = true;
            column.filtertype = 'reference';
            column.enablefilter = true;
            column.getValue = function (item, col) {
                var customer = $scope.SearchRef(item.customerid, $scope.Customers, '');
                if (item.shipto && item.shipto > 0) {
                    customer += ' - ' + $scope.SearchRef(item.shipto, $scope.Customers, '');
                }
                return customer;
            }

            var sourcelist = null;
            if (typeof (filterlist) != 'undefined')
                sourcelist = filterlist;
            else if ($scope.Mode == 'LIST')
                sourcelist = $scope.SOList;
            else
                sourcelist = $scope.SOSummary;

            if (sourcelist != null) {
                var hasdata = sourcelist.find(function (item) {
                    return item.salesmenid && item.salesmenid > 0;
                });
                if (hasdata && hasdata != null) {
                    igrid.AddReferenceColumn('salesmenid', 'Salesmen', 100, $scope.Employees);
                }
                var hasdata = sourcelist.find(function (item) {
                    return item.agentid && item.agentid > 0;
                });
                if (hasdata && hasdata != null) {
                    igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.Agent);
                }
                var hasdata = sourcelist.find(function (item) {
                    return item.cityid && item.cityid > 0;
                });
                if (hasdata && hasdata != null) {
                    igrid.AddReferenceColumn('cityid', 'City', 100, $scope.CityList);
                }
            }
            else {
                igrid.AddReferenceColumn('salesmenid', 'Salesmen', 100, $scope.Employees);
                igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.Agent);
                igrid.AddReferenceColumn('cityid', 'City', 100, $scope.CityList);
            }

            igrid.AddReferenceColumn('productid', 'Product', 100, $scope.ProductList);
            igrid.AddSimpleColumn('designitemcode', 'Design', 100).commafilter = true;

            igrid.AddReferenceColumn('createdby', 'Created', 100, $scope.Employees);
            igrid.AddSimpleColumn('remark', 'Remark', 80);
            igrid.AddSimpleColumn('sobookno', 'Book No', 80);
            if ($scope.mfg_customerpo)
            igrid.AddSimpleColumn('customerpo', 'PO No', 80);

            var slist = $scope.system_possizelist == '' ? [] : $scope.system_possizelist.split(',');
            if (slist == null || slist == undefined || slist.length == 0) {

                slist = [];
                $.each(sourcelist, function (k, v) {
                    $.each(v.sizelist, function (kk, vv) {
                        if (slist.indexOf(vv.id) < 0)
                            slist.push(vv.id);
                    });
                });
            }
            $.each(slist, function (k, v) {
                var qtyfield = 'q';
                if ($scope.SearchRequest.statusid == 6)
                    qtyfield = 'or';
                if ($scope.SearchRequest.statusid == 2)
                    qtyfield = 'op';
                if ($scope.SearchRequest.statusid == 3)
                    qtyfield = 'oc';
                if ($scope.SearchRequest.statusid == 4)
                    qtyfield = 'ob';
                var column = igrid.AddCustomColumn('size_' + k, v, 40, function (item, col) {
                    if (item.sizelist && item.sizelist.length > 0) {
                        var qty = 0;
                        $.each(item.sizelist, function (kk, vv) {
                            if (col.size == vv.id)
                                qty += vv[qtyfield];
                        })
                        return $filter('IndianNumber')(qty, 0);
                        //var sizeqty = item.sizelist.find(function (sitem) {
                        //    return sitem.id == col.size;
                        //})

                        //if (sizeqty && sizeqty.q && sizeqty.q> 0)
                        //    return $filter('IndianNumber')(sizeqty.q, 0)
                    }
                    return '';
                });
                column.cssClass = 'rightalign';
                column.size = v;
                column.sortable = true;
                column.enablefilter = true;
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.sizelist && item.sizelist.length > 0) {
                        var qty = 0;
                        $.each(item.sizelist, function (kk, vv) {
                            if (col.size == vv.id)
                                qty += vv[qtyfield];
                        })
                        return qty;
                    }
                    return 0;
                }
            })


            if ($scope.SearchRequest.statusid == 0) {
                //igrid.AddFormatedColumn('oqty', 'Ordered', 80, 'N0', 2).total = true;
                //igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;
                //igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;

                igrid.AddFormatedColumn('oqty', 'Ordered', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;

                var column = igrid.AddCustomColumn('st', 'Status', 40, function (item, col) {
                    return item.sostatus == 'C' ? 'Completed' : (item.sostatus == 'P' ? 'Partial' : 'New');
                });
                column.sortable = true;
                column.enablefilter = true;
                column.filtertype = 'reference';
            }
            else if ($scope.SearchRequest.statusid == 2) {
                igrid.AddFormatedColumn('plannedqty', 'Planned', 80, 'N0', 2).total = true;
            } else if ($scope.SearchRequest.statusid == 3) {
                igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
            } else if ($scope.SearchRequest.statusid == 4) {
                igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('oqty', 'Order', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('eqty', 'Extra', 80, 'N0', 2).total = true;
            }
            else if ($scope.SearchRequest.statusid == 6) {
                igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
            } else {
                igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
            }

             if ($scope.SearchRequest.statusid == 5) {
                var column = igrid.AddCustomColumn('st', 'Status', 40, function (item, col) {
                    return item.sostatus == 'C' ? 'Completed' : (item.sostatus == 'P' ? 'Partial' : 'New');
                });
                column.sortable = true;
                column.enablefilter = true;
                column.filtertype = 'reference';

                $.each(slist, function (k, v) {
                    var column = igrid.AddCustomColumn('stock_' + k, v, 40, function (item, col) {
                        if (item.sizelist && item.sizelist.length > 0) {
                            var sizeqty = item.sizelist.find(function (sitem) {
                                return sitem.id == col.size;
                            })

                            if (sizeqty && sizeqty.st && sizeqty.st > 0)
                                return $filter('IndianNumber')(sizeqty.st, 0)
                        }
                        return '';
                    });
                    column.cssClass = 'rightalign';
                    column.size = v;
                    column.sortable = true;
                    column.total = true;
                    column.enablefilter = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.sizelist && item.sizelist.length > 0) {
                            var sizeqty = item.sizelist.find(function (sitem) {
                                return sitem.id == col.size;
                            })

                            if (sizeqty && sizeqty.st && sizeqty.st > 0)
                                return sizeqty.st;
                        }
                        return 0;
                    }
                })
                igrid.AddFormatedColumn('possibleqty', 'Possible', 80, 'N0', 2).total = true;
                $.each(slist, function (k, v) {
                    var column = igrid.AddCustomColumn('planned_' + k, v, 40, function (item, col) {
                        if (item.sizelist && item.sizelist.length > 0) {
                            var sizeqty = item.sizelist.find(function (sitem) {
                                return sitem.id == col.size;
                            })

                            if (sizeqty && sizeqty.op && sizeqty.op > 0)
                                return $filter('IndianNumber')(sizeqty.op, 0)
                        }
                        return '';
                    });
                    column.cssClass = 'rightalign';
                    column.size = v;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.sizelist && item.sizelist.length > 0) {
                            var sizeqty = item.sizelist.find(function (sitem) {
                                return sitem.id == col.size;
                            })

                            if (sizeqty && sizeqty.op && sizeqty.op > 0)
                                return sizeqty.op;
                        }
                        return 0;
                    }
                })
                igrid.AddFormatedColumn('plannedqty', 'Planned', 80, 'N0', 2).total = true;
            }
            //igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('availableqty', 'Available', 80, 'N0', 2).total = false;
            //igrid.AddFormatedColumn('allocatedqty', 'Allocated', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('pipelinestock', 'WIP', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;
            //igrid.AddFormatedColumn('delivery', 'D.P %', 100, 'N2', 2).total = false;
            igrid.EnableTotal();
            igrid.Initialize();
            if ($scope.SearchRequest.statusid == 5) {
                igrid.setCellCSS('igridbigrows');
                igrid.dataView.getItemMetadata = function (row) {
                    var rowdata = igrid.grid.getData().getItem(row);

                    if (rowdata.plannedqty && rowdata.plannedqty > 0) {
                        if (rowdata.pendingqty == rowdata.plannedqty)
                            rowdata.style = 'igridbigrowswithgreen';
                        else
                            rowdata.style = 'igridbigrowswithdarkgreen';
                    } else if (rowdata.possibleqty && rowdata.possibleqty > 0) {
                        if (rowdata.possibleqty == rowdata.pendingqty)
                            rowdata.style = 'igridbigrows';
                        else
                            rowdata.style = 'igridbigrowswithmark';
                    }
                    else {
                        rowdata.style = 'igridbigrowswithred';
                    }
                    return { 'cssClasses': 'igridbigrows' };
                }
            }

            $scope.igrid = igrid;
            if (typeof (filterlist) != 'undefined')
                $scope.igrid.UpdateData(filterlist, '_id');
            else if ($scope.Mode == 'LIST')
                $scope.igrid.UpdateData($scope.SOList, '_id');
            else
                $scope.igrid.UpdateData($scope.SOSummary, '_id');

            igrid.grid.onDblClick.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'sono') {
                    var selectedIndexes = [];
                    for (var i = 0; i < args.grid.getDataLength(); i++) {
                        if (args.grid.getDataItem(i).sono == item.sono) {
                            if (item.style && args.grid.getDataItem(i).style) {
                                if (item.style == args.grid.getDataItem(i).style)
                                    selectedIndexes.push(i);
                            }
                            else {
                                selectedIndexes.push(i);
                            }
                        }
                    }
                    args.grid.setSelectedRows(selectedIndexes);
                    $timeout(function () {
                        args.grid.invalidateRows(selectedIndexes);
                        args.grid.render();
                    }, 10);
                }
            });
        }

        $scope.ShowList = function (summary) {
            $scope.DesignSummaryList = null;
            $scope.Mode = summary ? 'SUMMARY' : 'LIST';

            $.each($scope.SOList, function (k, v) {
                v.delivery = 0;
                if (v.availableqty > 0) {
                    if (v.availableqty >= v.pendingqty) {
                        v.delivery = 100;
                    }
                    else {
                        v.delivery = v.availableqty / v.pendingqty * 100;
                    }
                }
            });

            if (summary) {
                var summary = [];
                var summaryKey = [];
                $.each($scope.SOList, function (k, v) {
                    if (typeof (summaryKey[v.soid]) == 'undefined') {
                        var citem = (JSON.parse(JSON.stringify(v)));
                        citem.list = [];
                        if (citem.availableqty > citem.pendingqty)
                            citem.availableqty = citem.pendingqty;
                        summary.push(citem);
                        summaryKey[citem.soid] = citem;
                    }
                    else {
                        summaryKey[v.soid].plannedqty += v.plannedqty;
                        summaryKey[v.soid].pendingqty += v.pendingqty;
                        summaryKey[v.soid].availableqty += (v.availableqty > v.pendingqty ? v.pendingqty : v.availableqty);
                        summaryKey[v.soid].allocatedqty += v.allocatedqty;
                        summaryKey[v.soid].delivery += v.delivery;
                    }
                    summaryKey[v.soid].list.push(v);
                });
                $.each(summary, function (k, v) {
                    if (v.delivery > 0)
                        v.delivery = (v.delivery / v.list.length);
                });
                $scope.SOSummary = summary;
            }

            $scope.SOConfig();
        }

        $scope.SOConfig = function (filterlist) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sono', 'Order No', 100);
            if ($scope.mfg_customerpo)
            igrid.AddSimpleColumn('customerpo', 'PO No', 80);
            igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('customername', 'Customer', ($scope.Mode == 'LIST') ? 150 : 300);

            igrid.AddSimpleColumn('agentname', 'Agent', 150);
            igrid.AddSimpleColumn('cityname', 'City', 100);

            if ($scope.Mode == 'LIST') {
                igrid.AddSimpleColumn('productname', 'Product', 150);
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.sizegroup)
                    igrid.AddSimpleColumn('sizegroup', 'Size/Set', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.brand)
                    igrid.AddSimpleColumn('brandname', 'Brand', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.style)
                    igrid.AddSimpleColumn('stylename', 'Style', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.design)
                    igrid.AddSimpleColumn('designid', 'Design', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.colour)
                    igrid.AddSimpleColumn('colourname', 'Colour', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.size)
                    igrid.AddSimpleColumn('sizename', 'Size', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.pattern)
                    igrid.AddSimpleColumn('patternname', 'Pattern', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.sleeve)
                    igrid.AddSimpleColumn('sleevename', 'Sleeve', 100).commafilter = true;
                if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.fit)
                    igrid.AddSimpleColumn('fitname', 'Fit', 100).commafilter = true;
            }
            
            igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('availableqty', 'Available', 80, 'N0', 2).total = false;
            igrid.AddFormatedColumn('allocatedqty', 'Allocated', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('pipelinestock', 'WIP', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('removedqty', 'Removed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('delivery', 'D.P %', 100, 'N2', 2).total = false;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            if (typeof (filterlist) != 'undefined')
                $scope.igrid.UpdateData(filterlist, '_id');
            else if ($scope.Mode == 'LIST')
                $scope.igrid.UpdateData($scope.SOList, '_id');
            else
                $scope.igrid.UpdateData($scope.SOSummary, '_id');
        }




        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.PartialPlan = function () {

            var rows = this.igrid.GetSelectedRows();
            if (rows.length == 0) return;
            var selectedrows = this.igrid.GetSelectedRows();
            $rootScope.Prompt('Planned', 'Qty', function (pqty) {
                pqty = parseFloat(pqty);
                if (!isNaN(pqty)) {
                    $scope.FixedPlan(selectedrows, pqty);
                }
            }, rows[0].pendingqty);

        }

        $scope.ClearPlan = function () {
            var rows = []
            if ($scope.Mode == 'Summary') {
                $.each(this.igrid.GetSelectedRows(), function (k, v) {
                    if (v.list && v.list.length > 0) {
                        $.each(v.list, function (kk, vv) {
                            rows.push(vv);
                        });
                    }
                });
            }
            else {
                rows = this.igrid.GetSelectedRows();
            }
            var request = {
                "items": rows,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalesorderService/ClearPlan",
                data: srequest
            }).success(function (result) {
                $.each(rows, function (k, v) {
                    v.plannedqty = 0;
                });
                $scope.igrid.UpdateData($scope.SOList, '_id');
            }).error(function (result) {
            });

        }

        $scope.CompleteSO = function () {

            var rows = []
            if ($scope.Mode == 'Summary') {
                $.each(this.igrid.GetSelectedRows(), function (k, v) {
                    if (v.list && v.list.length > 0) {
                        $.each(v.list, function (kk, vv) {
                            rows.push(vv);
                        });
                    }
                });
            }
            else {
                rows = this.igrid.GetSelectedRows();
            }
            var request = {
                "items": rows,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalesorderService/CompleteSO",
                data: srequest
            }).success(function (result) {
                $.each(rows, function (k, v) {
                    v.pendingqty = 0;
                });
                $scope.igrid.UpdateData($scope.SOList, '_id');
            }).error(function (result) {
            });
        }
        $scope.CancelSO = function () {

            var rows = []
            if ($scope.Mode == 'Summary') {
                $.each(this.igrid.GetSelectedRows(), function (k, v) {
                    if (v.list && v.list.length > 0) {
                        $.each(v.list, function (kk, vv) {
                            rows.push(vv);
                        });
                    }
                });
            }
            else {
                rows = this.igrid.GetSelectedRows();
            }
            var request = {
                "items": rows,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalesorderService/CancelSO",
                data: srequest
            }).success(function (result) {
                $.each(rows, function (k, v) {
                    v.pendingqty = 0;
                });
                $scope.igrid.UpdateData($scope.SOList, '_id');
            }).error(function (result) {
            });
        }

        $scope.GetAvailableStock = function () {
            var stock = [];
            $.each($scope.SOList, function (k, v) {
                var existstock = stock.find(function (item) {
                    return item.productname == v.productname &&
                        item.designid == v.designid &&
                        item.colourname == v.colourname &&
                        item.sizegroup == v.sizegroup;
                });
                if (typeof (existstock) == 'undefined' || existstock == null) {
                    existstock = {
                        'productname': v.productname,
                        'designid': v.designid,
                        'colourname': v.colourname,
                        'sizegroup': v.sizegroup,
                        'stock': v.availableqty
                    };
                    stock.push(existstock);
                }
                existstock.stock -= v.allocatedqty;
            });
            return stock;
        }

        $scope.UIDFixedPlan = function (rows, defaultqty) {

            var rows = []
            var stocklist = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                if (v.sizelist && v.sizelist.length > 0) {
                    $.each(v.sizelist, function (kk, vv) {
                        var stock = stocklist.find(function (item) {
                            return item.designitemcode == v.designitemcode &&
                                item.sizeid == vv.i;
                        });
                        if (stock && stock != null) {
                            stock.qty += vv.st;
                        }
                        else {
                            stocklist.push({ 'sizeid': vv.i, 'stock': vv.st, 'designitemcode': v.designitemcode });
                        }
                        rows.push({
                            'sizeid': vv.i, 'pending': vv.q, 'planned': vv.pl,
                            'designitemcode': v.designitemcode, 'ids': vv.sid, 'soid': v.soid
                        });
                    });
                }
            });
            rows.sort(function (a, b) {
                var first = parseInt(a.ids.split(',')[0]);
                var second = parseInt(a.ids.split(',')[0]);
                if (first < second) { return -1; }
                if (first > second) { return 1; }
                return 0;
            });
            var requestrows = [];
            $.each(rows, function (kk, vv) {
                var stock = stocklist.find(function (item) {
                    return item.sizeid == vv.sizeid && item.designitemcode == vv.designitemcode;
                });
                if (stock && stock != null && stock.stock > 0) {
                    var available = vv.pending - vv.planned;
                    if (available < stock.stock) {
                        vv.qty = available;
                    }
                    else {
                        vv.qty = stock.stock;
                    }
                    stock.stock -= vv.qty;
                    requestrows.push({ 'soid': vv.soid, 'ids': vv.ids, 'plannedqty': vv.qty });
                }
            })

            if (requestrows.length > 0) {
                var request = {
                    "items": requestrows,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SalesorderService/ChangePlan",
                    data: srequest
                }).success(function (result) {
                    $scope.SearchSOSummary();
                }).error(function (result) {

                });
            }

        }

        $scope.FixedPlan = function (rows, defaultqty) {
            if (rows == null)
                rows = this.igrid.GetSelectedRows();

            var rows = []
            if ($scope.Mode == 'Summary') {
                $.each(this.igrid.GetSelectedRows(), function (k, v) {
                    if (v.list && v.list.length > 0) {
                        $.each(v.list, function (kk, vv) {
                            rows.push(vv);
                        });
                    }
                });
            }
            else {
                rows = this.igrid.GetSelectedRows();
            }

            var stock = $scope.GetAvailableStock();
            var stocknotavailable = [];
            var missingstock = 0;
            var plannedstock = 0;
            if (typeof (defaultqty) == 'undefined') defaultqty = 0;
            $.each(rows, function (k, v) {

                var required = v.pendingqty - v.plannedqty;
                if (required > 0 && defaultqty > 0) {
                    if (required > defaultqty) {
                        required = defaultqty;
                    }
                }
                if (required > 0) {
                    var existstock = stock.find(function (item) {
                        return item.productname == v.productname &&
                            item.designid == v.designid &&
                            item.colourname == v.colourname &&
                            item.sizegroup == v.sizegroup;
                    });
                    if (typeof (existstock) != 'undefined' && existstock != null
                        && existstock.stock > 0) {
                        if (existstock.stock > required) {
                            v.plannedqty += required;
                            existstock.stock -= required;
                            plannedstock += required;
                        }
                        else {
                            v.plannedqty += existstock.stock;
                            existstock.stock = 0;
                            plannedstock += existstock.stock;
                        }
                    }
                    else {
                        stocknotavailable.push({
                            'productname': v.productname,
                            'designid': v.designid,
                            'colourname': v.colourname,
                            'sizegroup': v.sizegroup,
                            'required': required
                        });
                        missingstock += required;
                    }
                }
            });

            if (rows.length > 0) {
                var request = {
                    "items": rows,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SalesorderService/ChangePlan",
                    data: srequest
                }).success(function (result) {

                    if (missingstock > 0)
                        $rootScope.Toaster.Info(missingstock + " items skipped.");
                    if (plannedstock > 0)
                        $rootScope.Toaster.Info(plannedstock + " items planned.");

                    $scope.SearchSOSummary();
                }).error(function (result) {

                });
            }
            else {
                $rootScope.Toaster.Info(missingstock + " items not planned becuase of stock availablity.");
            }
        }

        $scope.ExportPivot = function () {


            var worksheetName = 'Quanto Report';

            var processRow = function (row) {
                var finalVal = '';
                for (var j = 0; j < row.length; j++) {
                    var innerValue = row[j] === null ? '' : row[j].toString();
                    if (row[j] instanceof Date) {
                        innerValue = row[j].toLocaleString();
                    };
                    var result = innerValue.replace(/"/g, '""');
                    if (result.search(/("|,|;|\n)/g) >= 0)
                        result = '"' + result + '"';
                    if (j > 0)
                        finalVal += ',';
                    finalVal += result;
                }
                return finalVal + '\n';
            };

            var csvFile = '';
            var rows = [];
            rows.push('<Thead>')
            var colname = [];
            var skipIndex = [];
            var formator = [];

            var colcount = 0;
            var columns = [
                { title: 'City', field: 'cityname', 'group': true, 'last': '' },
                { title: 'Customer', field: 'customername', 'group': true, 'last': '' },
                { title: 'Product', field: 'productname', 'group': true, 'last': '' },
                { title: 'Design', field: 'designid', 'group': false, 'last': '' },
                { title: 'Colour', field: 'colourname', 'group': false, 'last': '' },
                { title: 'Size/Set', field: 'sizegroup', 'group': false, 'last': '' },
                { title: 'Pending', field: 'pendingqty', 'group': false, 'last': '' },
                { title: 'Available', field: 'availableqty', 'group': false, 'last': '' }
            ];
            var sortingfields = {
                cityname: true, customername: true, customername: true
                , productname: true, designid: true, colourname: true
                , sizegroup: true
            } // This is used as a dictionary. Sort by "name" DESC and "age" ASC

            var datarows = this.igrid.GetSelectedRows();
            if (datarows.length == 0)
                datarows = $scope.SOList;

            datarows.sort((a, b) => {
                for (col in sortingfields) { // added properties are positional
                    if (a[col] != b[col]) { // if they are equal, sorting is made on the next column
                        if (sortingfields[col] === true) return a[col] > b[col] // if ASC === true
                        else return a[col] < b[col]
                    }
                }
            })


            rows.push('<Tr>')
            for (var j = 0, len = columns.length; j < len; j++) {
                colname.push('<th>');
                colname.push(columns[j].title);
                colname.push('</th>');
            }
            rows.push(colname.join(''));
            rows.push('</Tr></Thead><Tbody>')

            var singlerow = [];
            for (var i = 0, l = datarows.length; i < l; i++) {
                for (var j = 0, len = columns.length; j < len; j++) {
                    singlerow.push('<td>');
                    var item = datarows[i];
                    var column = columns[j];
                    var value = item[column.field];
                    if (value == null) value = '';
                    if (column.group) {
                        if (column.last == value) {
                            singlerow.push('');
                        }
                        else {
                            singlerow.push(value);
                            column.last = value;
                        }
                    }
                    else {
                        singlerow.push(value);
                    }
                    singlerow.push('</td>');
                }
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
            var ctx = { worksheet: worksheetName, table: tableContent };


            var blob = new Blob([format(template, ctx)], { type: 'application/vnd.ms-excel;charset=utf-8;' });
            if (navigator.msSaveBlob) { // IE 10+
                navigator.msSaveBlob(blob, worksheetName + ".xls");
                return null;
            } else {
                var link = document.createElement("a");
                if (link.download !== undefined) { // feature detection
                    // Browsers that support HTML5 download attribute
                    var url = URL.createObjectURL(blob);
                    link.setAttribute("href", url);
                    link.setAttribute("download", worksheetName + ".xls");
                    link.style.visibility = 'hidden';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    return null;
                }
            }

            href = uri + base64(format(template, ctx));

            var link = document.createElement("a");
            link.setAttribute("href", href);
            link.setAttribute("download", "filename.csv");
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }


        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'SALES ORDER - INFORMATION'
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

        $scope.uanpofileuploadedcomplete = function (result) {
            $scope.uploadfile = null;
            $scope.UANSO = result;
            var dialog = ngDialog.open({
                template: 'ImportSO',
                data: {
                    'OrderInfo': result,
                    'ParentScope': $scope
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.OrderInfo = $scope.ngDialogData.OrderInfo;
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Orders = $scope.OrderInfo.sostatus.orders;
                    $scope.Count = 0;
                    $scope.SelectedAll = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            if (v.valid) {
                                v.selected = $scope.selectedall;
                                if ($scope.selectedall)
                                    $scope.Count += 1;
                            }
                        });

                    }
                    $scope.SelectedChanged = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            $scope.Count += (typeof (v.selected) != 'undefined' && v.selected) ? 1 : 0;
                        });
                    }
                    $scope.ShowReason = function (item) {
                        $.alert(item.FailReason);
                    }
                    $scope.Save = function () {
                        var selected = [];
                        $.each($scope.Orders, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected)
                                selected.push(v);
                        });
                        $scope.closeThisDialog(selected);
                    };
                    $scope.MissingStock = function () {


                        var data = [];
                        data.push([
                            'UAN',
                            'STOCK',
                            'SO',
                            'STO',
                            'POSSIBLE',
                            'REQUIRED',
                            'ORDERS'
                        ]);

                        var stock = 0;
                        var required = 0;
                        var so = 0;
                        var sto = 0;
                        var possible = 0;
                        $.each($scope.OrderInfo.sostatus.stocklist, function (k, v) {
                            data.push([
                                v.uan,
                                v.stock,
                                v.so,
                                v.sto,
                                (v.stock - (v.so + v.sto)),
                                v.required,
                                v.orders.join(',')
                            ]);
                            stock += v.stock;
                            so += v.so;
                            possible += (v.stock - (v.so + v.sto));
                            sto += v.sto;
                            required += v.required;
                        });

                        data.push([
                            'Total',
                            stock,
                            so,
                            sto,
                            possible,
                            required,
                            '',
                        ]);

                        if (data.length > 0) {

                            var title = {									// set the style for target cell
                                font: {
                                    name: 'verdana',
                                    sz: 11,
                                    bold: true,
                                    color: { rgb: "33220000" }
                                },
                                alignment: {
                                    vertical: "center",
                                    horizontal: "center"
                                },
                                border: {
                                    top: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    bottom: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                }
                            };

                            var Stitle = {									// set the style for target cell
                                font: {
                                    name: 'verdana',
                                    sz: 11,
                                    bold: true,
                                    color: { rgb: "33220000" }
                                },
                                alignment: {
                                    vertical: "center",
                                    horizontal: "right"
                                },
                                border: {
                                    top: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    bottom: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                }
                            };
                            var boderstyle = {
                                border: {
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                },
                            }

                            var ws = XLSX.utils.aoa_to_sheet(data);
                            var wb = XLSX.utils.book_new();

                            ws["A1"].s = title;
                            ws["B1"].s = title;
                            ws["C1"].s = title;
                            ws["D1"].s = title;
                            ws["E1"].s = title;
                            ws["F1"].s = title;
                            ws["G1"].s = title;

                            ws["A" + data.length].s = Stitle;
                            ws["B" + data.length].s = Stitle;
                            ws["C" + data.length].s = Stitle;
                            ws["D" + data.length].s = Stitle;
                            ws["E" + data.length].s = Stitle;
                            ws["F" + data.length].s = Stitle;
                            ws["G" + data.length].s = Stitle;

                            ws['!cols'] = [{ width: 20 }, { width: 10 }, { width: 10 }, { width: 10 }, { width: 15 }, { width: 15 }, { width: 50 }];

                            var prefix = '';
                            var startfrom = 64;
                            for (var ridx = 1; ridx <= 7; ridx++) {
                                startfrom++;
                                for (var rnidx = 2; rnidx < data.length; rnidx++) {
                                    ws[prefix + String.fromCharCode(startfrom) + rnidx].s = boderstyle;
                                }
                                if (startfrom == 90) {
                                    charcode = 65;
                                    if (prefix != '') {
                                        var charcode = prefix.charCodeAt(prefix.length - 1);
                                        charcode++;
                                        if (charcode > 90) {
                                            charcode = 65;
                                        }
                                    }
                                    prefix = String.fromCharCode(charcode);
                                    startfrom = 64;
                                }
                            }

                            XLSX.utils.book_append_sheet(wb, ws, "Stock");
                            const wbout = XLSX.write(wb, { type: "array", bookType: "xlsx" });
                            saveAs(new Blob([wbout], { type: "application/octet-stream" }), "stockshortage.xlsx");
                        }
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {

                        $scope.UANSO.sostatus.orders = data.value;
                        var request = {
                            "item": $scope.UANSO.sostatus,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "SalesorderService/SaveUANSO",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Total imported Orders - " + result.length);
                        });

                    }
                }
            });

        }

        $scope.ShowDesignList = function () {
            var dialog = ngDialog.open({
                template: 'ChooseDesign',
                data: {
                    'restService': $scope.restService,
                    'currentlist': $scope.SearchRequest.design
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.restService = $scope.ngDialogData.restService;
                    $scope.currentlist = $scope.ngDialogData.currentlist;
                    if (typeof ($scope.currentlist) == 'undefined' || $scope.currentlist == null || $scope.currentlist == '')
                        $scope.currentlist = [];
                    else
                        $scope.currentlist = $scope.currentlist.split(',');

                    $scope.DesignList = restService.Get('MfgdesignService/SearchByFabricDesign/-1', true).then(function (results) {
                        $scope.DesignList = results;
                        if ($scope.currentlist.length > 0) {
                            $.each($scope.currentlist, function (k, v) {
                                var exist = results.find(function (item) { return item.code == v; });
                                if (exist && exist != null)
                                    exist.selected = true;
                            })
                        }
                    });
                    $scope.Count = 0;
                    $scope.SelectedChanged = function () {
                        $scope.Count = 0;
                        $.each($scope.DesignList, function (k, v) {
                            $scope.Count += (typeof (v.selected) != 'undefined' && v.selected) ? 1 : 0;
                        });
                    }
                    $scope.Search = function () {
                        var selected = [];
                        $.each($scope.DesignList, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected)
                                selected.push(v.code);
                        });
                        $scope.closeThisDialog({ 'selected': selected });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        if (data.value.selected.length > 0) {
                            $scope.SearchRequest.design = data.value.selected.join(',');
                            $scope.SearchSOSummary();
                        }
                    }
                }
            });

        }

        $scope.StatusChange = function (action) {

            var items = $scope.GetSelecteItemsstatus();
            if (items.length == 0)
                return;

            var inboxitems = [];
            $.each(items, function (k, v) {
                var inboxstatus = { 'id': v.id, 'hasclosed': false };
                inboxstatus.statusname = action;

                if (action == "Working") {
                    inboxstatus.statusid = 1;
                }
                else if (action == "Completed") {
                    inboxstatus.statusid = 2;
                    inboxstatus.hasclosed = true;
                }
                else if (action == "Cancel") {
                    inboxstatus.statusid = 3;
                }
                inboxitems.push(inboxstatus);
            });

            request = {
                "Items": inboxitems,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "InboxService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $.each(items, function (k, v) {
                    $scope.LRInboxList.splice($scope.LRInboxList.indexOf(v), 1);
                });
            }).error(function (result) {
            });
        };

        $scope.LoadDefault = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadDefault();
                }, 50);
                return;
            }
            $scope.SearchRequest.statusid = 1;
            var today = new Date();
            var beginfrom = new Date((today).setDate(today.getDate() - 14));
            $scope.SearchRequest.fromdate = $filter('date')(beginfrom, datePicker.shortDisplay);
            $scope.SearchRequest.todate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;
            if ($scope.system_possizelist == undefined || $scope.system_possizelist == null) $scope.system_possizelist = '';
            $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
            $scope.mfg_customerpo = $rootScope.UserInfo.appsettings.system_mfg_customerpo;
            $scope.mfg_mode = $rootScope.UserInfo.appsettings.mfg_mode;
            $scope.SearchSOSummary();
        }
        $scope.LoadDefault();
    }]);
