
var app = angular.module('InfinityLab');

app.controller('EcomInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$filter', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $filter, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.filteredtype = '';
        $scope.ApproveItem = null;
        $scope.igrid = null;
        $scope.SelectedCount = 0;
        $scope.StockAdjustment = false;

        $scope.DateTypes = [{ 'id': 'DISPATCHDATE', 'name': 'DISPATCH DATE' },
        { 'id': 'ORDERDATE', 'name': 'ORDER DATE' }];

        $scope.StatusList = [{ 'id': 'ALL', 'name': 'ALL' }, { 'id': 'COMPLETE', 'name': 'COMPLETE' },
        { 'id': 'PROCESSING', 'name': 'PROCESSING' },
        { 'id': 'CANCELLED', 'name': 'CANCELLED' },
        { 'id': 'RETURNS', 'name': 'RETURNS' }];


        $scope.BillStatusList = [{ 'id': 'ALL', 'name': 'ALL' }, { 'id': 'UNBILLED', 'name': 'UNBILLED' },
        { 'id': 'BILLED', 'name': 'BILLED' },
        { 'id': 'RETURNS', 'name': 'RETURNS' }];

        restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
        });

        $scope.Company = null;
        $scope.Location = null;
        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.SearchRequest = {
            addedfrom: null,
            addedto: null,
            status: -1,
        };


        $scope.SearchBundle = function () {
            if ($scope.SearchRequest.locationid && $scope.SearchRequest.locationid > 0 &&
                $scope.SearchRequest.bundlecode && $scope.SearchRequest.bundlecode != ''
            ) {
            request = {
                "Item": {
                    'bundlecode': $scope.SearchRequest.bundlecode,
                    'locationid': $scope.SearchRequest.locationid
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EcombillService/SearchBundle",
                data: srequest
            }).success(function (results) {
                if (results == undefined || results == null || results.length == 0) return;

                $.each(results, function (k, v) {
                    var eitem = $scope.LRInboxList.find(function (item) {
                        return item.uan == v.uan;
                    });
                    if (eitem && eitem != null) {
                        eitem.add = v.add;
                        eitem.remove = v.remove;
                    }
                })
                $scope.igrid.UpdateData($scope.LRInboxList, '_id');
            }).error(function (result) {
            });
            }
        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: $scope.SearchRequest.Online ? "EcombillService/SearchOnline" : "EcombillService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ConfigGrid();
                $scope.LRInboxList = result;
                $scope.igrid.UpdateData(result, '_id');
            }).error(function (result) {
            });
        }

        $scope.SearchStock = function () {
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EcombillService/SearchStock",
                data: srequest
            }).success(function (result) {
                $scope.ConfigStockGrid();
                $scope.LRInboxList = result;
                $scope.igrid.UpdateData(result, '_id');
            }).error(function (result) {
            });
        }

        $scope.ProcessOnline = function () {
            request = {
                "Items": this.igrid.GetSelectedRows(),
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EcombillService/ProcessOnline",
                data: srequest
            }).success(function (result) {
                $scope.uanpofileuploadedcomplete(result);
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Unicommerce Sales Order'
            infygrid.ExportXLSX(infygrid.grid, title, subtitle, true, []);
        }

        $scope.ConfigStockGrid = function (addremove) {

            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 40);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('uan', 'UAN', 100);
            igrid.AddSimpleColumn('prdsku', 'SKU', 200);

            igrid.AddFormatedColumn('stk', 'Stock', 100, 'N0', 0).total = true;

            if (addremove || $scope.StockAdjustment) {
                var column = igrid.AddColumn(
                    {
                        id: "add", name: "Add", field: "add",
                        width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, filtertype: 'number', enablefilter:true
                    }
                );
                column.cssClass = 'rightalign';
                column.total = true;
                column.format = 'N2';
                column.decimal = 0;
                column.cssClass = "rightalign";
                column.formatter = Slick.Formatters.IndianNumberFormatter;
                column.getValue = function (item, col) {
                    if (item.add && item.add != null)
                        return item.add;
                    return 0;
                }

                var column = igrid.AddColumn(
                    {
                        id: "remove", name: "Remove", field: "remove",
                        width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, filtertype: 'number', enablefilter: true
                    }
                );
                column.cssClass = 'rightalign';
                column.total = true;
                column.format = 'N2';
                column.decimal = 0;
                column.cssClass = "rightalign";
                column.formatter = Slick.Formatters.IndianNumberFormatter;
                column.getValue = function (item, col) {
                    if (item.remove && item.remove != null)
                        return item.remove;
                    return 0;
                }

                var column = igrid.AddColumn(
                    {
                        id: "replace", name: "Replace", field: "replace",
                        width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, filtertype: 'number', enablefilter: true
                    }
                );
                column.cssClass = 'rightalign';
                column.total = true;
                column.format = 'N2';
                column.decimal = 0;
                column.cssClass = "rightalign";
                column.formatter = Slick.Formatters.IndianNumberFormatter;
                column.getValue = function (item, col) {
                    if (item.replace && item.replace != null)
                        return item.replace;
                    return 0;
                }

            }
            else {
                igrid.AddFormatedColumn('inventory', 'Inventory', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('openSale', 'Open Sale', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('openPurchase', 'Open Purchase', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('putawayPending', 'Putaway Pending', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('inventoryBlocked', 'Blocked', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('pendingStockTransfer', 'Pending Transfer', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('vendorInventory', 'Vendor Inventory', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('virtualInventory', 'Virtyal Inventory', 100, 'N0', 0).total = true;
                igrid.AddFormatedColumn('pendingInventoryAssessment', 'Pending Assessment', 100, 'N0', 0).total = true;
            }

            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();

            if (addremove) {
                igrid.onSelectedRangesChanged = function (range, rowmodel) {
                    $timeout(function () {
                        $scope.SelectedCount = range.length;
                    }, 10)
                };

                igrid.dataView.getItemMetadata = function (row) {
                    var item = igrid.dataView.getItem(row);
                    if (typeof (item.updated) != 'undefined') {
                        if (item.updated)
                            return { cssClasses: 'igridcreditnoterow' };
                        else
                            return { cssClasses: 'igriddebitnoterow' };
                    }
                };
            }
            $scope.igrid = igrid;
            $scope.SelectedCount = 0;
        }

        $scope.ConfigGrid = function () {

            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 40);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('channel', 'Chennal', 100);
            igrid.AddSimpleColumn('statuscode', 'Status', 80);
            igrid.AddFormatedColumn('orderdate', 'Order Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('dispatchon', 'Dispatch Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoicecode', 'Invoice Code', 100);
            igrid.AddSimpleColumn('shipcode', 'Ship Code', 100);
            igrid.AddSimpleColumn('orderno', 'Order No', 100);
            igrid.AddSimpleColumn('externorderno', 'External No', 100);
            if ($scope.SearchRequest.status != 'UNBILLED') {
                igrid.AddSimpleColumn('billno', 'Quanto Bill', 100);
            }
            if ($scope.SearchRequest.status == 'BILLED') {
                igrid.AddSimpleColumn('imported', 'Imported By', 150);
                igrid.AddFormatedColumn('importedon', 'Imported Date', 80, 'dd-MM-yyyy');
            }

            igrid.AddSimpleColumn('customername', 'Customer Name', 150);
            igrid.AddFormatedColumn('lineamount', 'Order Amout', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('unitprice', 'Unit Price', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('maxretailprice', 'MRP', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('orderqty', 'Order Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('voucheramount', 'Voucher Value', 80, 'N2', 0).total = true;
            igrid.AddSimpleColumn('address', 'Address', 80);
            igrid.AddSimpleColumn('shipstate', 'State', 80);
            igrid.AddSimpleColumn('sku', 'SKU', 80);
            igrid.AddSimpleColumn('itemname', 'Item Name', 80);
            igrid.AddSimpleColumn('color', 'Colour', 80);
            igrid.AddSimpleColumn('size', 'Size', 80);
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onDblClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id.startsWith('shipstate')) {
                    item = args.grid.getDataItem(args.row);
                    if (item.id && item.id > 0)
                        $scope.ChangeState(item);
                }
            });

            $scope.igrid = igrid;
            $scope.SelectedCount = 0;
            //            if ($scope.SearchRequest.Online) {
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.SelectedCount = range.length;
                }, 10)
            };
            //            }
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Manage')
                    $scope.ManageLR(item, function (itm) {
                        item.version = itm.version;
                        $scope.igrid.referesh();
                    });
                if (command == 'Print')
                    $scope.PrintBill(item);
                if (command == 'Delivery Slip')
                    $scope.PrintDeliverySlip(item);
                if (command == 'Edit') {
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.EditBill(item, function (itm) {
                            itm.billvalue = itm.total;
                            $scope.igrid.referesh();
                        });
                    }
                }
                if (command == 'Cancel')
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.CancelComformation(item, false);
                    }
                if (command == 'Remove')
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.CancelComformation(item, true);
                    }
                if (command == 'Change No')
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.ChangeBillNo(item);
                    }
            });
            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if (item.hascancel) {
                    item.style = 'igridbigrowswithred';
                    return { cssClasses: 'igridbigrowswithred' };
                }
                else {
                    if (item.status == 1) {
                        item.style = item.status == 1 ? 'igridbigrowswithmark' : '';
                        return { cssClasses: 'igriddebitnoterow' };
                    }
                    else {
                        item.style = item.packedby == 0 ||
                            item.transportid == 0 ||
                            item.lrno == '' ? 'igridwithgrey' : '';
                        return { cssClasses: '' };
                    }
                }
            };
            $scope.igrid.UpdateData([]);
        }

        $scope.ChangeState = function (item) {
            $rootScope.Prompt('State', 'Enter the statename other than ' + item.shipstate, function (value) {
                item.shipstate = value;
                request = {
                    "Item": item,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "EcombillService/Update",
                    data: srequest
                }).success(function (result) {
                    item.shipstate = result.shipstate;
                    $scope.igrid.referesh();
                }).error(function (result) {
                });
            });
        }

        $scope.uanpofileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            $scope.UANPoList = value;
            var dialog = ngDialog.open({
                template: 'ImportPO',
                data: {
                    'OrderInfo': value,
                    'ParentScope': $scope
                },
                width: '600px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.OrderInfo = $scope.ngDialogData.OrderInfo;
                    $scope.stockfailed = $scope.OrderInfo.ecomstatus.stocklist && $scope.OrderInfo.ecomstatus.stocklist.length > 0;
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Orders = $scope.OrderInfo.ecomstatus.orders;
                    $scope.ShowBillNo = $scope.OrderInfo.ecomstatus.ShowBillNo;
                    $scope.Count = 0;
                    $scope.Running = false;
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

                    $scope.MissingStock = function () {
                        var data = [];
                        data.push([
                            'UAN',
                            'STOCK',
                            'SO',
                            'STO',
                            'POSSIBLE',
                            'REQUIRED'
                        ]);

                        var stock = 0;
                        var required = 0;
                        var so = 0;
                        var sto = 0;
                        var possible = 0;
                        $.each($scope.OrderInfo.ecomstatus.stocklist, function (k, v) {
                            data.push([
                                v.uan,
                                v.stock,
                                v.so,
                                v.sto,
                                (v.stock - (v.so + v.sto)),
                                v.required
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

                            ws["A" + data.length].s = Stitle;
                            ws["B" + data.length].s = Stitle;
                            ws["C" + data.length].s = Stitle;
                            ws["D" + data.length].s = Stitle;
                            ws["E" + data.length].s = Stitle;
                            ws["F" + data.length].s = Stitle;

                            ws['!cols'] = [{ width: 20 }, { width: 10 }, { width: 10 }, { width: 10 }, { width: 15 }, { width: 15 }];

                            var prefix = '';
                            var startfrom = 64;
                            for (var ridx = 1; ridx <= 6; ridx++) {
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

                    $scope.CheckStatus = function () {
                        restService.Get('File/ProcessStatus/' + $scope.UserInfo.OrganizationID + '/' + $scope.StatusID).then(function (result) {
                            $scope.UploadedingStatus = result;
                            $scope.ProgressValue = result.percentage;
                            if (!result.hascompleted) {
                                $scope.migrationtimer = $timeout(function () {
                                    $scope.CheckStatus();
                                }, 2000);
                            }
                            else {
                                var scount = 0;
                                var fcount = 0;
                                if (result.result && result.result != null) {
                                    $.each(result.result, function (k, v) {
                                        var ord = $scope.Orders.find(function (item) {
                                            return item.MOrderNo == v.sid;
                                        });
                                        if (v.id > 0) {
                                            scount++;
                                            if (ord && ord != null) {
                                                ord.errormessage = 'Imported';
                                            }
                                        }
                                        else {
                                            fcount++;
                                            if (ord && ord != null) {
                                                ord.errormessage = v.errormessage;
                                            }
                                        }
                                    })
                                    if (result.statuslist && result.statuslist != null && result.statuslist.length > 0) {
                                        $.alert("Total imported Orders - " + result.result.length + " Success - " + scount + " Failed - " + fcount + '<br>' +
                                            result.statuslist.join(' <br> '));
                                    }
                                    else {
                                        $.alert("Total imported Orders - " + result.result.length + " Success - " + scount + " Failed - " + fcount);
                                    }
                                }
                                else {
                                    $.alert("Completed");
                                }
                            }
                        });
                    }

                    $scope.Save = function () {
                        var selected = [];
                        var skipped = [];
                        $.each($scope.Orders, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected) {
                                if (v.valid) {
                                    selected.push(v);
                                }
                                else {
                                    skipped.push(v);
                                }
                            }
                        });
                        if (skipped.length > 0) {
                            $rootScope.Toaster.Info("Invalid orders can't import");
                        }
                        else {
                            $scope.Running = true;
                            $scope.OrderInfo.ecomstatus.orders = selected;
                            $scope.UploadedingStatus = { 'totalrows': selected.length, 'failedrows': 0, 'completedrows': 0, 'status': 'Starting' };
                            var request = {
                                "item": $scope.OrderInfo.ecomstatus,
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "BillinboxService/SaveUANECOM",
                                data: srequest
                            }).success(function (result) {
                                $scope.StatusID = result.identifier;
                                $scope.migrationtimer = $timeout(function () {
                                    $scope.CheckStatus();
                                }, 2000);
                            });
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                }
            });
        }

        $scope.Unicommerce2Stock = function () {
            var request = {
                "Items": [],
                "SecurityToken": "String content"
            }

            var rows = this.igrid.GetSelectedRows();
            $.each(rows, function (k, v) {
                request.Items.push({
                    'prdsku': v.prdsku,
                    'add': v.add,
                    'remove': v.remove,
                    'replace': v.replace
                })
            });

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EcombillService/Stock2Unicommerce",
                data: srequest
            }).success(function (results) {

                $.each(results, function (k, v) {
                    var eitem = $scope.LRInboxList.find(function (item) {
                        return item.prdsku == v.prdsku;
                    });
                    if (eitem && eitem != null) {
                        eitem.updated = v.updated;
                    }
                });
                $scope.ConfigStockGrid(true);
                $scope.igrid.UpdateData($scope.LRInboxList, '_id');

            }).error(function (result) {
            });
        }

        $scope.EnableStockAdjustment = function () {
            $scope.StockAdjustment = true;
            $scope.ConfigStockGrid(true);
            $scope.igrid.UpdateData($scope.LRInboxList, '_id');
        }

        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
                $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
                $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                $scope.Unicommerce = $rootScope.UserInfo.appsettings.int_unicommerce;
            } else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 10);
            }
        }

        $timeout(function () {
            $scope.LoadAppSetting();
        }, 10);
    }]);
