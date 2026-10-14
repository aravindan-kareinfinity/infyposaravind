var app = angular.module('InfinityLab');

app.controller('PurchaseOrderGRNController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    'ngDialog', '$state', '$timeout', '$filter', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter, cntxService) {
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
        $scope.hasbarcodegenerated = false;
        $scope.EntryTypes = [{
            'id': -1,
            'name': 'None',
            'discount': true,
            'amount': true,
            'tax': false,
            'directamount': false
        },
        {
            'id': 0,
            'name': 'Bill Value',
            'discount': true,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 1,
            'name': 'Tax',
            'discount': false,
            'amount': false,
            'tax': true,
            'directamount': false
        },
        {
            'id': 2,
            'name': 'Discount',
            'discount': true,
            'amount': false,
            'tax': false,
            'directamount': false
        },
        {
            'id': 3,
            'name': 'OnDiscount',
            'discount': true,
            'amount': false,
            'tax': false,
            'directamount': false
        },
        {
            'id': 4,
            'name': 'Insurance',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 5,
            'name': 'Service',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 6,
            'name': 'Courier',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 7,
            'name': 'Packing',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 8,
            'name': 'Fright',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 9,
            'name': 'Rounding',
            'discount': true,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 10,
            'name': 'Job Work',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        }
        ];

        $scope.Suppliers = restService.Get('ReferencelistService/LIST-SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.EmployeeList = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.EmployeeList = results;
        });


        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'All Locations' });
        });


        $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
        });

        restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });

        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });

        $scope.SearchRequest = {
            searchtype: 0,
            locationid: 0,
            statusid: 1
        };

        $scope.SearchType = [
            { 'id': 0, 'name': 'Default' },
            { 'id': 1, 'name': 'Supplier' },
            { 'id': 3, 'name': 'City' },
            { 'id': 4, 'name': 'Product' },
            { 'id': 5, 'name': 'Design' }];

        $scope.StatusList = [{ 'id': 0, 'name': 'Unapproved' },
        { 'id': 1, 'name': 'Approved' },
        { 'id': 2, 'name': 'Rejected' },
        { 'id': 3, 'name': 'Completed' },
        { 'id': 6, 'name': 'GRN' },
        { 'id': 7, 'name': 'Billed' },
        { 'id': 8, 'name': 'Scanned' },
        { 'id': 9, 'name': 'Warehouse' }];

        $scope.SOList = [];
        $scope.LocalContext = cntxService.CreatePageContext('PurchasezOrderGRN');
        $scope.SearchPOSummary = function () {
            $scope.totalrecords = 0;
            $scope.SOList = [];
            $scope.SearchRequest.potype = -5;
            request = {
                "item": $scope.SearchRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchPOSummary",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SOList = result;
                $scope.ShowList(result);
            }).error(function (result) {
            });
        }

        $scope.SelecteOrder = null;

        $scope.ShowList = function (result) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            $scope.selectedcount = 0;
            $scope.SelecteOrder = null;
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('pono', 'Order No', 100);
            igrid.AddFormatedColumn('podate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expirydate', 'Expiry Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('suppliername', 'Supplier', 100);
            igrid.AddSimpleColumn('suppliercity', 'From Location', 100);
            igrid.AddSimpleColumn('remarks', 'Remarks', 100);
            igrid.AddReferenceColumn('internalstatus', 'Status', 100, $scope.StatusList);
            var column = igrid.AddCustomColumn('actionemployee', 'Approval', 150, function (item, col) {
                if (item.status == 2)
                    return 'Rejected - ' + item.actionemployee;
                if (item.status > 0)
                    return 'Approved - ' + item.actionemployee;
                return '';
            });
            column.sortable = true;
            column.filtertype = 'reference';
            column.enablefilter = true;
            column.getValue = function (item, col) {
                if (item.status == 2)
                    return 'Rejected - ' + item.actionemployee;
                if (item.status > 0)
                    return 'Approved - ' + item.actionemployee;
                return '';
            }

            igrid.AddReferenceColumn('deliverylocationid', 'To Location', 100, $scope.Location);
            igrid.AddFormatedColumn('ordered', 'Ordered', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('balance', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('cancelled', 'Cancelled', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('received', 'Billed', 80, 'N0', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SOList, '_id');
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    if (range.length == 1) {
                        $scope.SelecteOrder = $scope.igrid.GetSelectedRows()[0];
                    }
                    else {
                        $scope.SelecteOrder = null;
                    }
                }, 10);
            };
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


        $scope.Search = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                if (result && result.lrid == -1 && result.errormessage) {
                    $rootScope.Toaster.Error(result.errormessage);
                    return;
                }
                $scope.InvoiceCover = result;
                $scope.Invoice = result.Items[0];
                $scope.Invoice.addtionalcostpercentage = result.addtionalcostpercentage;
                $scope.Invoice.additionalamount = result.additionalamount;
                $scope.Invoice.additionaltax = result.additionaltax;
                $scope.Invoice.premargin = result.premargin;
                $scope.Invoice.internalbuyer = result.internalbuyer;
                $scope.Invoice.lrid = result.lrid;
                $scope.Invoice.lrno = result.lrno;
                $scope.Invoice.frightcharge = result.frightcharge;
                $scope.Invoice.loadingcharge = result.loadingcharge;
                $scope.Invoice.lrentryno = result.lrentryno;
                $scope.Invoice.invoiceno = result.Items[0].invoiceno;
                $scope.Invoice.status = result.status;
                $scope.Invoice.supplierid = result.supplierid;
                $scope.Invoice.suppliercompanyname = result.suppliercompanyname;
                $scope.Invoice.suppliergstno = result.suppliergstno;
                $scope.Invoice.companyname = result.companyname;
                $scope.Invoice.retaillocationid = result.retaillocationid;
                $scope.Invoice.lrnoInfo = result.lrentryno + " / " + result.lrno;
                $scope.ConfigGrid();
            }).error(function (result) { });
        }

        $scope.tgrid = null;
        $scope.egrid = null;
        $scope.SearchProductRef = function (invoiceitem) {
            if (invoiceitem.Details.sellingname == '' || invoiceitem.Details.sellingname == null) {
                if (invoiceitem.Details.productname != null && invoiceitem.Details.productname != '')
                    return invoiceitem.Details.productname;
            }
            if (invoiceitem.Details.sellingname != null || invoiceitem.Details.sellingname != '') {

            }
            return invoiceitem.Details.sellingname;
        }

        $scope.barcodeuploadedcomplete = function (values) {
            $scope.uploadfile = null;
            var barcodecolumn = -1;
            var qtycolumn = -1;
            var barcodelist = [];
            $.each(values.columns, function (k, v) {
                if (v.toLowerCase().indexOf('barcode') >= 0 || v.toLowerCase().indexOf('uan') >= 0 || v.toLowerCase().indexOf('gln') >= 0) {
                    barcodecolumn = k;
                }
                if (v.toLowerCase().indexOf('qty') >= 0) {
                    qtycolumn = k;
                }
            });
            $.each(values.rows, function (k, v) {
                if (k > 0) {
                    var exist = barcodelist.find(function (item) {
                        return item.barcode == v.values[barcodecolumn];
                    });
                    if (exist == undefined || exist == null) {
                        var exist = { 'barcode': v.values[barcodecolumn], 'qty': 0 };
                        barcodelist.push(exist);
                    }
                    if (qtycolumn >= 0) {
                        exist.qty += parseInt(v.values[qtycolumn]);
                    }
                    else {
                        exist.qty += 1;
                    }
                }
            });

            var missingbarcodes = [];
            var failedbarcodes = [];
            $.each(barcodelist, function (k, v) {
                var uandetail = $scope.Invoice.Lrinvoiceitems.find(function (item) {
                    return item.uan == v.barcode;
                });
                if (uandetail == undefined || uandetail == null) {
                    missingbarcodes.push(v);
                }
                else {
                    uandetail.haschanged = true;
                    var tqty = 0;
                    var oqty = 0;
                    if (uandetail.poinfo && uandetail.poinfo != null) {
                        $.each(uandetail.poinfo, function (kk, vv) {
                            tqty += $scope.GetNumber(vv.tqty);
                            oqty += $scope.GetNumber(vv.oqty);
                        });
                    }
                    if (uandetail.actualqty + v.qty > tqty) {
                        if (failedbarcodes.indexOf(uandetail.uan) < 0)
                            failedbarcodes.push(uandetail.uan);
                    }
                    else {
                        uandetail.actualqty += v.qty;
                        $scope.Total.actualqty += v.qty;
                    }
                }
            });

            if (failedbarcodes.length > 0) {
                $.alert("Can't add above the tolerance qty of tolerance - " + failedbarcodes.join(','));
            }

            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems);
            $scope.LocalContext.Post('$scope.Invoice', $scope.Invoice, false);
        }

        $scope.ValidateTolerance = function () {
            if ($scope.Total.toleranceqty > 0) {
                if ($scope.Total.toleranceqty < $scope.Total.actualqty) {
                    $.alert("Can't add above the tolerance qty of " + $scope.Total.toleranceqty);
                    return false;
                }
            }
            return true;
        }

        $scope.Save = function (iscomplete) {

            if (iscomplete) {
                if (!$scope.ValidateTolerance())
                    return;
            }

            if (iscomplete)
                $scope.Invoice.statusid = 3;

            $scope.Invoice.internalstatus = $scope.internalstatus;
            if ($scope.internalstatus == 7) {
                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                    var buyingqty = v.buyingqty;
                    if (v.poinfo && v.poinfo != null) {
                        if (v.poinfo.length == 1) {
                            v.poinfo[0].aqty = buyingqty;
                        }
                        else {
                            $.each(v.poinfo, function (kk, vv) {
                                vv.aqty = vv.oqty;
                                buyingqty -= vv.qty;
                                if (buyingqty < 0) {
                                    v.poinfo[0].aqty += (buyingqty * -1);
                                    buyingqty = 0;
                                }
                            });
                        }
                    }
                })
            }

            if ($scope.internalstatus == 8)
                $scope.Invoice.internalstatus = iscomplete ? 9 : 8;

            var request = {
                "Item": $scope.Invoice,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SaveInvoiceItems",
                data: srequest
            }).success(function (result) {
                $scope.Invoice.version = result.version;
                if (iscomplete)
                    $rootScope.Toaster.Info("Barcode Generated, Stock is available for Invoicing");
                else if ($scope.internalstatus == 7 && !iscomplete)
                    $rootScope.Toaster.Info("GRN Items Updated");
                else if ($scope.internalstatus == 8 && iscomplete)
                    $rootScope.Toaster.Info("Scanning Updated");
            });
        }

        $scope.AddAllSelectedAsScanned = function () {
            $.each($scope.egrid.GetSelectedRows(), function (k, v) {
                if (v.actualqty == undefined || v.actualqty == null)
                    v.actualqty = 0;
                $scope.Total.actualqty += v.buyingqty - v.actualqty;
                v.actualqty = v.buyingqty;
            })
            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems);
            $scope.LocalContext.Post('$scope.Invoice', $scope.Invoice, false);
        }



        $scope.CreateAutoDebitNotes = function () {
            var request = {
                "Item": $scope.Invoice,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/CreateAutoDebitNotes",
                data: srequest
            }).success(function (result) {
                $scope.Invoice.version = result.version;
            });
        }

        $scope.ConfigGrid = function () {
            if ($scope.tgrid != null)
                $scope.tgrid.destroy();
            var igrid = new InfyGrid('taxgrid',
                ($scope.internalstatus == '7') ? '400px' : '150px', '100%', true, false, true,
                ($scope.internalstatus == '7') ? 160 : 400);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('type', 'Type', 80, $scope.EntryTypes);
            igrid.AddFormatedColumn('percentage', '%', 50, 'N2', 2);
            igrid.AddFormatedColumn('amounton', 'Amount', 60, 'N2', 2);
            igrid.AddFormatedColumn('discount', 'Discnt', 50, 'N2', 2);
            igrid.AddFormatedColumn('discountpercentage', '%', 40, 'N2', 2);
            igrid.AddFormatedColumn('taxamount', 'Tax', 50, 'N2', 2);
            igrid.AddFormatedColumn('netamount', 'Total', 70, 'N2', 2);
            igrid.Initialize();
            igrid.UpdateData($scope.Invoice.Lrinvoicetaxitems, '_id');
            $scope.tgrid = igrid;

            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('gridList', '450px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);

            igrid.AddReferenceColumn('productid', 'Products', 50, $scope.ProductList);

            var column = igrid.AddCustomColumn('sizeid', 'Size', 70, function (item, col) {
                return $scope.SearchRef(item.Details.sizeid, $scope.SizeList);
            });

            var column = igrid.AddCustomColumn('colourid', 'Colour', 70, function (item, col) {
                return $scope.SearchRef(item.Details.colourid, $scope.ColorList);
            });

            var column = igrid.AddColumn({
                id: "hsncode",
                name: "HSN",
                field: "hsncode",
                width: 60,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });

            column = igrid.AddFormatedColumn('purchasetaxpercentage', 'Tax %', 60, 'N2', 2);

            var column = igrid.AddCustomColumn('uan', 'UAN', 100, function (item, col) {
                return item.Details.serialno;
            });
            column.filtertype = 'reference';
            column.enablefilter = true;

            groupColumns = [];
            groupColumns.push(
                {
                    id: 'Detail',
                    name: 'Detail',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];

            var column = igrid.AddCustomColumn('orderqty', 'Qty', 50, function (item, col) {
                if (item.poinfo && item.poinfo != null) {
                    var oqty = 0;
                    $.each(item.poinfo, function (k, v) {
                        oqty += $scope.GetNumber(v.oqty);
                    });
                    return oqty;
                }
                return '';
            });
            column.cssClass = 'rightalign';

            var column = igrid.AddCustomColumn('tolerance', 'Tolerance', 70, function (item, col) {
                if (item.poinfo && item.poinfo != null) {
                    var tqty = 0;
                    $.each(item.poinfo, function (k, v) {
                        tqty += $scope.GetNumber(v.tqty);
                    });
                    return tqty;
                }
                return '';
            });
            column.cssClass = 'rightalign';

            var column = igrid.AddCustomColumn('orderprice', 'Cost', 60, function (item, col) {
                if (item.poinfo && item.poinfo != null) {
                    var oqty = 0;
                    var price = 0;
                    $.each(item.poinfo, function (k, v) {
                        oqty += $scope.GetNumber(v.oqty);
                        price += ($scope.GetNumber(v.oqty) * $scope.GetNumber(v.price));
                    });
                    return (price / oqty).round(2);
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column = igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2);
            groupColumns.push(
                {
                    id: 'Order',
                    name: 'Order',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];

            if ($scope.internalstatus == '7') {
                var column = igrid.AddColumn({
                    id: "buyingqty",
                    name: "Qty",
                    field: "buyingqty",
                    width: 60,
                    cssClass: "cell-title",
                    editor: Slick.Editors.Text,
                    'sortable': true
                });
                column.cssClass = 'rightalign';
            }
            else {
                var column = igrid.AddColumn({
                    id: "buyingqty",
                    name: "Qty",
                    field: "buyingqty",
                    width: 60,
                    cssClass: "cell-title",
                    'sortable': true
                });
                column.cssClass = 'rightalign';
            }

            var column = igrid.AddColumn({
                id: "basebuyingprice",
                name: "Cost",
                field: "basebuyingprice",
                width: 60,
                cssClass: "cell-title",
                'sortable': true
            });
            column.cssClass = 'rightalign';


            igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2).total = true;
            groupColumns.push(
                {
                    id: 'Invoice',
                    name: 'Invoice',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            if ($scope.internalstatus == '8') {
                igrid.columns = [];
                igrid.AddFormatedColumn('actualqty', 'Qty', 80, 'N2', 2).total = true;
                groupColumns.push(
                    {
                        id: 'Scanned',
                        name: 'Scanned',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;

            $scope.selectedcount = 0;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();

            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                if (rowdata.haschanged)
                    rowdata.style = 'igridbigrowswithgreen';
                else if (rowdata.sellableqty > 0)
                    rowdata.style = 'igridbigrowswithred';
                else
                    rowdata.style = '';
                return { 'cssClasses': rowdata.style };
            }

            $scope.egrid = igrid;
            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'buyingqty') {
                    $scope.QtyChanged(item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });
            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems);
            $scope.ComputeTotal();

        }

        $scope.Rescan = function (all) {
            if (all) {
                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                    v.actualqty = 0;
                })
                $scope.Total.actualqty = 0;
            }
            else {
                $scope.Total.actualqty -= $scope.LastScanned.actualqty;
                $scope.LastScanned.actualqty = 0;
            }
            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems);
        }

        $scope.LastScanned = null;
        $scope.SearchUAN = function () {
            var uandetail = $scope.Invoice.Lrinvoiceitems.find(function (item) {
                return item.uan == $scope.Invoice.UAN;
            });
            if (uandetail == undefined || uandetail == null) {
                $.alert("Can't find the UAN - " + $scope.Invoice.UAN);
                $scope.Invoice.UAN = '';
                return;
            }
            if ($scope.LastScanned != null)
                $scope.LastScanned.haschanged = false;

            var tqty = 0;
            var oqty = 0;
            if (uandetail.poinfo && uandetail.poinfo != null) {
                $.each(uandetail.poinfo, function (kk, vv) {
                    tqty += $scope.GetNumber(vv.tqty);
                    oqty += $scope.GetNumber(vv.oqty);
                });
            }

            if (uandetail.actualqty + 1 > tqty) {
                $.alert("Can't add above the tolerance qty of " + tqty);
                return;
            }

            if ($scope.Total.toleranceqty > 0) {
                if ($scope.Total.toleranceqty < ($scope.Total.actualqty + 1)) {
                    $.alert("Can't add above the tolerance qty of " + $scope.Total.toleranceqty);
                    return;
                }
            }


            $scope.Invoice.UAN = '';
            uandetail.haschanged = true;
            uandetail.actualqty += 1;

            $scope.Total.actualqty += 1;
            $scope.LastScanned = uandetail;
            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems);
            $scope.LocalContext.Post('$scope.Invoice', $scope.Invoice, false);
        }

        $scope.SaveQCStatus = function () {

            var dialog = ngDialog.open({
                template: 'QCApproveDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.QC = restService.Get('LrinvoiceService/GetQCApproveval/' + $scope.parent.Invoice.id, true).then(function (results) {
                        $scope.QC = results;
                    });
                    $scope.Save = function () {
                        $scope.closeThisDialog($scope.QC);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var webrequest = {
                            "item": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "LrinvoiceService/SetQCApproveval",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                $rootScope.Toaster.Error('QC successfully changed.');
                                $scope.Invoice.qcby = data.value.qcby;
                            }
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.Invoice', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }

            if (value.Lrinvoiceitems && value.Lrinvoiceitems != null) {
                $.each(value.Lrinvoiceitems, function (k, v) {
                    var currentitem = $scope.Invoice.Lrinvoiceitems.find(function (item) {
                        return item.id == v.id;
                    });
                    if (currentitem && currentitem != null) {
                        currentitem.amount = v.amount;
                        currentitem.purchasetax = v.purchasetax;
                        currentitem.buyingqty = v.buyingqty;
                        currentitem.actualqty = v.actualqty;

                        if (v.haschanged) {
                            $scope.LastScanned = currentitem;
                            $scope.LastScanned.haschanged = true;
                        }
                    }
                })
            }

            if (value.Lrinvoicetaxitems && value.Lrinvoicetaxitems != null) {
                $.each(value.Lrinvoicetaxitems, function (k, v) {
                    var currentitem = $scope.Invoice.Lrinvoicetaxitems.find(function (item) {
                        return item.id == v.id;
                    });
                    if (currentitem && currentitem != null) {
                        currentitem.amounton = v.amounton;
                        currentitem.taxamount = v.taxamount;
                        currentitem.netamount = v.netamount;
                    }
                })
            }
            $scope.ComputeTotal();
            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
            $scope.tgrid.UpdateData($scope.Invoice.Lrinvoicetaxitems, '_id');
        };

        $scope.ComputeTotal = function () {
            $scope.hasbarcodegenerated = false;
            $scope.Total = { 'amount': 0, 'tax': 0, 'cesstax': 0, 'discount': 0, 'buyingqty': 0, 'actualqty': 0, 'value': 0, 'toleranceqty': 0, 'orderqty': 0 }
            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                $scope.Total.amount += v.amount;
                $scope.Total.tax += v.purchasetax;
                $scope.Total.value += v.purchasetax + v.amount;
                $scope.Total.buyingqty += $rootScope.GetNumber(v.buyingqty);
                $scope.Total.actualqty += $rootScope.GetNumber(v.actualqty);
                if (v.sellableqty && v.sellableqty > 0)
                    $scope.hasbarcodegenerated = true;
                if (v.poinfo && v.poinfo != null) {
                    $.each(v.poinfo, function (kk, vv) {
                        $scope.Total.toleranceqty += $scope.GetNumber(vv.tqty);
                        $scope.Total.orderqty += $scope.GetNumber(vv.oqty);
                    });
                }
            })
        }

        $scope.ProductBarcode = function () {
            $location.url('/barcode?lrid=' + $scope.Invoice.lrid);
        };

        $scope.Print = function (key) {
            if (key == 'PURCHASEINVOICE')
                $rootScope.Download("LrinvoiceService/PrintGRNPDF/" + $scope.UserInfo.OrganizationID + '/' + $scope.Invoice.id, 'VendorGRN.pdf', 'application/pdf');
            else
                $rootScope.Download("LrinvoiceService/PrintDCPNYPDF/" + $scope.UserInfo.OrganizationID + '/' + $scope.Invoice.id, 'VendorDiscrepancy.pdf', 'application/pdf');
        }

        $scope.QtyChanged = function (item) {
            var oldamount = item.amount;
            var oldpurchasetax = item.purchasetax;
            item.amount = $rootScope.GetNumber(item.buyingqty) * item.buyingprice;
            item.purchasetax = (item.amount * item.purchasetaxpercentage / 100).round(4);

            var taxitem = $scope.Invoice.Lrinvoicetaxitems.find(function (titem) {
                return titem.taxid == item.ptaxid && titem.percentage == item.purchasetaxpercentage;
            });

            if (taxitem && taxitem != null) {
                taxitem.amounton = taxitem.amounton - oldamount + item.amount;
                taxitem.taxamount = taxitem.taxamount - oldpurchasetax + item.purchasetax;
                taxitem.netamount = taxitem.amounton + taxitem.taxamount;
                $scope.tgrid.UpdateData($scope.Invoice.Lrinvoicetaxitems, '_id');
            }
            $scope.ComputeTotal();
            $scope.LocalContext.Post('$scope.Invoice', $scope.Invoice, false);
        }

        $scope.importreferencetype = 0;
        $scope.importreferenceid = 0;
        $scope.internalstatus = 0;
        $scope.LoadDefault = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadDefault();
                }, 50);
                return;
            }
            $scope.SearchRequest.fromdate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.SearchRequest.todate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;
            if ($scope.system_possizelist == null) $scope.system_possizelist = '';
            $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;

            if (typeof ($location.search().importreferenceid) != 'undefined' &&
                typeof ($location.search().importreferencetype) != 'undefined') {

                $scope.importreferencetype = $location.search().importreferencetype;
                $scope.importreferenceid = $location.search().importreferenceid;
                $scope.internalstatus = $location.search().internalstatus;

                $scope.SearchCondition = {
                    companyid: 0,
                    supplierid: 0,
                    invoiceno: "",
                    addedfrom: null,
                    addedto: null,
                    lrid: 0,
                    importreferencetype: $location.search().importreferencetype,
                    importreferenceid: $location.search().importreferenceid,
                }
                $scope.Search();
            }

        }
        $scope.LoadDefault();
    }]);
