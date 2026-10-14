var app = angular.module('InfinityLab');

app.controller('WOGrnController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('SEARCH');
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = true;
        $scope.SaveRequest = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = '';
        $scope.WorkOrder = null;
        $scope.mfg_fabricbasedorder = true;


        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.SearchCondition = {
            companyid: 0,
            supplierid: 0,
            invoiceno: "",
            addedfrom: null,
            addedto: null,
            lrentryno: ""
        }
        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.JobCharges = restService.Get('ReferencelistService/JOBCHARGE', true).then(function (results) {
            $scope.JobCharges = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.Location = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.RetailLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.RetailLocation = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo == null || !v.additionalinfo.iswarehouse) {
                    $scope.RetailLocation.push(v);
                }
            });
            $scope.AllRetailLocation = $scope.RetailLocation;
        });

        $scope.WarehouseLocations = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                if ($scope.WorkOrder != null && $scope.WorkOrder.locationid > 0) {
                    var lastId = $scope.WorkOrder.locationid;
                    $scope.WorkOrder.locationid = 0;
                    $timeout(function () {
                        $scope.WorkOrder.locationid = lastId;
                    }, 20);
                }
            }

            $scope.mfg_fabricbasedorder = $rootScope.UserInfo.appsettings.mfg_fabricbasedorder;
            $scope.mfg_grnlrinvoice = $rootScope.UserInfo.appsettings.mfg_grnlrinvoice;
            if (!$scope.mfg_fabricbasedorder) {
                $scope.mfg_designclass = "col-md-6";
                restService.Get('MfgdesignService/SearchAll', true).then(function (results) {
                    $scope.DesignList = results;
                });
            }
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });

        $scope.AllColours = [];
        $scope.ColourList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColourList = results;
            $scope.AllColours = results;
        });

        $scope.MfgFlowList = restService.Get('ReferencelistService/MFGWOFLOW', true).then(function (results) {
            $scope.MfgFlowList = results;
        });

        $scope.PurchaseTax = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.purchaseTaxActual = results;
            var taxlist = [];
            $.each(results, function (k, v) {
                if (typeof (v.taxsplit) == 'undefined' || typeof (v.taxsplit.items) == 'undefined' || v.taxsplit.items == null) {
                    var taxitem = (JSON.parse(JSON.stringify(v)));
                    taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                    taxitem.code = '' + taxitem.id + '_' + 1;
                    taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage, 'sgst': 0 }] };
                    taxitem.type = 0;
                    taxlist.push(taxitem);
                }
                else {
                    $.each(v.taxsplit.items, function (kk, vv) {
                        var taxitem = (JSON.parse(JSON.stringify(v)));
                        taxitem.taxpercentage = vv.cgst + vv.sgst;
                        taxitem.type = 0;
                        if (v.taxsplit.items.length > 1)
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '% (' + vv.fromamount + ' - ' + vv.toamount + ')';
                        else
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxlist.push(taxitem);
                    });
                }
            })
            $scope.PurchaseTax = taxlist;

            $scope.PurchaseTax.push({
                'id': -1, 'name': 'Rounding', 'taxpercentage': 'Rounding', 'type': 9
            });

            $scope.JobChargeChanged = function () {

            }

            $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
                $.each(results, function (k, v) {

                    var taxitem = $scope.purchaseTaxActual.find(function (item) {
                        return item.id == v.referenceid;
                    });

                    var taxitem = (JSON.parse(JSON.stringify(taxitem)));
                    taxitem.name = v.name + ' @ ' + taxitem.taxpercentage + '%';
                    taxitem.code = 'Ad_' + taxitem.id + '_' + 1;
                    if (taxitem.taxpercentage > 0)
                        taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage / 2, 'sgst': taxitem.taxpercentage / 2 }] };
                    else
                        taxitem.taxsplit = { 'items': [{ 'cgst': 0, 'sgst': 0 }] };
                    taxitem.type = 8;
                    $scope.PurchaseTax.push(taxitem);
                });

            });
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                if ($scope.WorkOrder != null && $scope.WorkOrder.companyid > 0) {
                    var lastId = $scope.WorkOrder.companyid;
                    $scope.WorkOrder.companyid = 0;
                    $timeout(function () {
                        $scope.WorkOrder.companyid = lastId;
                    }, 20);
                }
            }
        });
        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });
        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                if ($scope.WorkOrder != null && $scope.WorkOrder.suppliercompanyid > 0) {
                    var lastId = $scope.WorkOrder.suppliercompanyid;
                    $scope.WorkOrder.suppliercompanyid = 0;
                    $timeout(function () {
                        $scope.WorkOrder.suppliercompanyid = lastId;
                    }, 20);
                }
            }
        });

        $scope.FabricList = restService.Get('ReferencelistService/FABRIC', true).then(function (results) {
            $scope.FabricList = results;
        });

        $scope.ItemTemplate = {
            "tax": 0,
            "taxid": null,
            "readonly": false,
            "isactive": true,
            "version": 1,
            "designid": "",
            "sno": 1,
        }
        $scope.ChangeQty = function (option) {
            if (option == 'All') {
                $.each($scope.currentsizeorders, function (k, v) {
                    v.qty = v.aqty - v.dqty;
                })
            }
            else {
                $.each($scope.currentsizeorders, function (k, v) {
                    v.qty = 0;
                })
            }

            if ($scope.SizeGrid) {
                $scope.ShowEditBySizeGrid(50);
            }
            else {
                $scope.ShowEditGrid(50);
            }
                

            $scope.CalculateTotal();
        }

        $scope.WorkOrderTemplate = {
            "companyid": 2, "inward2warehouse": true,
            "grossamount": 0.0, "totalamount": 0.0, "id": 0, "modifiedon": "", "createdon": "",
            "createdby": 0, "modifiedby": 0, "version": 1, "suppliercompanyid": 0, "isactive": true,
            "totalqty": 0, "taxable": 0, "tax": 0, "total": 0,
            Mfgworkordergrnitems: [], sizelist: [], receivinglocationid: 0, locationid: 0
        }

        $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
        $scope.WorkOrder = (JSON.parse(JSON.stringify($scope.WorkOrderTemplate)));

        $scope.currentfabricid = 0;
        $scope.fabricChange = function () {
            if (typeof ($scope.ItemDetails.fabricid) == 'undefined' ||
                $scope.currentfabricid == $scope.ItemDetails.fabricid)
                return;

            restService.Get('MfgdesignService/SearchByFabric/' + $scope.ItemDetails.fabricid, true).then(function (results) {
                $scope.DesignList = results;
                $scope.currentfabricid = $scope.ItemDetails.fabricid;
                $timeout(function () {
                    $scope.setFocusdeisgnid();
                }, 10);
            });
        }


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

        $scope.ChangeCR = function (type) {
            var inqty = $scope.GetNumber($scope.ItemDetails.inqty);
            var cutratio = $scope.GetNumber($scope.ItemDetails.cutratio);
            if (type == 'cutratio' || type == 'initialize' || type == 'inqty') {
                $scope.ItemDetails.received = Math.floor(inqty / cutratio);
            }

            var totqty = 0;
            var allqty = 0;
            $.each($scope.ItemDetails.sizelist, function (k, v) {
                var cutratio = $scope.GetNumber(v.cutratio);
                if (!isNaN(cutratio))
                    totqty += cutratio;
            });

            if (type == 'sizeqty') {
                $.each($scope.ItemDetails.sizelist, function (k, v) {
                    var qty = $scope.GetNumber(v.qty);
                    if (!isNaN(qty)) {
                        allqty += qty;
                    }
                });
                $scope.ItemDetails.received = allqty;
                $scope.ItemDetails.cutratio = (inqty / allqty).toFixed(2);
            }
            else {
                var maxqty = 0;
                var maxindex = -1;
                $.each($scope.ItemDetails.sizelist, function (k, v) {
                    var cutratio = $scope.GetNumber(v.cutratio);
                    if (!isNaN(cutratio)) {
                        v.qty = Math.round(cutratio / totqty * $scope.ItemDetails.received);
                        allqty += v.qty;
                        if (k == 0 || v.qty > maxqty) {
                            maxindex = k;
                            maxqty = v.qty;
                        }
                    }
                });
                if (allqty != $scope.ItemDetails.received) {
                    if ($scope.ItemDetails.received > allqty) {
                        $scope.ItemDetails.sizelist[0].qty += ($scope.ItemDetails.received - allqty);
                    }
                    else {
                        $scope.ItemDetails.sizelist[maxindex].qty -= (allqty - $scope.ItemDetails.received);
                    }
                }
            }
        }

        $scope.ShowQty = function (list, size) {
            var value = list.find(function (item) {
                return item.sizeid == size.sizeid;
            });
            if (typeof (value) == 'undefined' || value == null)
                return '';

            return value.qty;
        }

        $scope.igrid = null;
        $scope.sgrid = null;
        $scope.ConfigureSearchGrid = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchgridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddFormatedColumn('entrydate', 'E.Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('entryno', 'E.No', 80);
            igrid.AddSimpleColumn('workorderno', 'O.No', 80);
            igrid.AddFormatedColumn('workorderdate', 'O.Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('design', 'Design', 80);
            igrid.AddSimpleColumn('product', 'Product', 80);
            igrid.AddFormatedColumn('totalqty', 'Qty', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('total', 'total', 80, 'N2', 0).total = true;
            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Manage'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"
            commands += "<input type='button' class='btn btn-success btn-xs' value='Print'/>"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Manage')
                        $scope.ManageSearchItem('EditWorkOrder', args.item);
                    else if ($(e.target).val() == 'Delete') 
                        $scope.ManageSearchItem('DeleteWorkOrder', args.item);
                    else if ($(e.target).val() == 'Print') 
                            $scope.Print(args.item.id);
                });
            });
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult);
        }

        $scope.currentsizeorders = [];
        $scope.ConfigureGrid = function (additionalheight) {
            $scope.currentsizeorders = [];
            var sno = 1;
            $.each($scope.WorkOrder.Mfgworkordergrnitems, function (k, v) {
                $.each(v.sizelist, function (kk, vv) {
                    vv.sno = sno++;
                    vv.designname = v.designname;
                    vv.designitemcode = v.designitemcode;
                    vv.colourid = v.colourid;
                    vv.sleeveid = v.sleeveid;
                    //vv.cost = vv.cost;
                    vv.taxpercentage = v.taxpercentage;
                    vv.taxid = v.taxid;
                    vv.productid = v.productid;
                    $scope.currentsizeorders.push(vv);
                })
            })
            $scope.ShowEditGrid(additionalheight);
        }

        $scope.SizeGrid = false;
        $scope.ShowEditBySizeGrid = function (additionalheight) {
            $scope.SizeGrid = true;
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('egridContainer', '425px', '100%', true, true, true, additionalheight);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 50);
            if (!$scope.mfg_fabricbasedorder) {
                igrid.AddReferenceColumn('productid', 'Product', 100, $scope.ProductList);
            }

            igrid.AddSimpleColumn('designitemcode', 'Design', 80);
            igrid.AddSimpleColumn('uid', 'UID', 80);
            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColours);
            igrid.AddReferenceColumn('sleeveid', 'Sleeve', 100, $scope.SleeveList);
            var newSlizeList = [];
            $.each($scope.SizeList, function (k, v) {
                var existsize = $scope.currentsizeorders.find(function (item) {
                    return item.sizeid == v.id;
                });

                if (existsize && existsize != null)
                    newSlizeList.push(v);
            })

            var currentsizelist = [];
            $.each($scope.currentsizeorders, function (k, v) {
                var designitem = currentsizelist.find(function (item) {
                    return item.designitemcode == v.designitemcode;
                });
                if (designitem && designitem != null) {
                    designitem.items.push(v);
                    designitem.tqty += v.qty;
                    v['size_' + v.sizeid] = v.qty;
                    v['aqty_' + v.sizeid] = v.aqty;
                }
                else {
                    designitem = (JSON.parse(JSON.stringify(v))); 
                    designitem.items = [v];
                    designitem.tqty = v.qty;
                    v['size_' + v.sizeid] = v.qty;
                    v['aqty_' + v.sizeid] = v.aqty;
                    currentsizelist.push(designitem);
                }
            })

            $.each(newSlizeList, function (k, v) {
                var column = igrid.AddColumn(
                    {
                        id: "qty_" + v.id, name: v.name, field: "qty_" + v.id,
                        width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.getValue = function (item, col) {
                    var sizeid = parseInt(col.id.split('_')[1]);
                    var sizeitem = item.items.find(function (sitem) {
                        return sitem.sizeid == sizeid
                    });
                    if (sizeitem && sizeitem != null)
                        return sizeitem.qty;

                    return 'X'
                };
                column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                    var sizeid = columnDef.internal.id;
                    var sizeitem = dataContext.items.find(function (sitem) {
                        return sitem.sizeid == sizeid;
                    });
                    if (sizeitem && sizeitem != null) {
                        if (sizeitem.aqty == 0)
                            return sizeitem.qty;
                        return '' + sizeitem.qty + '(' + sizeitem.aqty + ')';
                    }
                    return 'X';
                }
                column.internal = v;
                column.cssClass = 'rightalign';
            });

            
            igrid.AddFormatedColumn('tqty', 'Total', 60, 'N2', 0);

            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var colid = args.grid.getColumns()[args.cell].id;
                var sizeid = parseInt(colid.split('_')[1]);

                var sizeitem = item.items.find(function (sitem) {
                    return sitem.sizeid == sizeid
                });

                if (sizeitem && sizeitem != null) {
                    item.tqty -= sizeitem.qty;
                    sizeitem.qty = parseFloat(item[colid]);
                    item.tqty += sizeitem.qty;
                } else {
                    item[colid] = 'X';
                }

                $timeout(function () {

                    $scope.CalculateTotal();
                    args.grid.updateRow(args.row);
                    $scope.egrid.UpdateAllTotals(args.grid);

                }, 10);

            });

            $scope.egrid = igrid;
            $scope.egrid.UpdateData(currentsizelist);
        }

        $scope.ShowEditGrid = function (additionalheight) {
            $scope.SizeGrid = false;
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('egridContainer', '425px', '100%', true, true, true, additionalheight);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 50);

            if (!$scope.mfg_fabricbasedorder) {
                igrid.AddReferenceColumn('productid', 'Product', 100, $scope.ProductList);
            }

            igrid.AddSimpleColumn('designitemcode', 'Design', 80);
            igrid.AddSimpleColumn('uid', 'UID', 80);
            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColours);
            igrid.AddReferenceColumn('sleeveid', 'Sleeve', 100, $scope.SleeveList);
            igrid.AddReferenceColumn('sizeid', 'size', 60, $scope.SizeList);
            igrid.AddFormatedColumn('eqty', 'Ordered', 60, 'N2', 0);
            igrid.AddFormatedColumn('aqty', 'Ready', 60, 'N2', 0);
            igrid.AddFormatedColumn('dqty', 'Delivered', 60, 'N2', 0);

            var column = igrid.AddColumn(
                {
                    id: "cost", name: "Cost", field: "cost",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            var column = igrid.AddColumn(
                {
                    id: "qty", name: "Actual", field: "qty",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            var column = igrid.AddColumn(
                {
                    id: "sqty", name: "Seconds", field: "sqty",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            var column = igrid.AddColumn(
                {
                    id: "cqty", name: "Wastage", field: "cqty",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onCellChange.subscribe(function (e, args) {

                $timeout(function () {

                    $scope.CalculateTotal();
                    args.grid.updateRow(args.row);
                    $scope.egrid.UpdateAllTotals(args.grid);

                }, 10);

            });

            $scope.egrid = igrid;
            $scope.egrid.UpdateData($scope.currentsizeorders);
        }

        $scope.SearchLrInfo = function () {
            if ($scope.mfg_grnlrinvoice) {
                var request = {
                    "Item": {
                        "lrentryno": $scope.WorkOrder.lrentryno
                    },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "LrentryService/SearchLast",
                    data: srequest
                }).success(function (result) {
                    if (result.id == 0) {
                        $rootScope.Toaster.Info("Please enter correct lr entry no");
                    }
                    else {
                        $scope.WorkOrder.lrno = result.lrno;
                        $scope.WorkOrder.lrentryid = result.id;
                        $scope.WorkOrder.lrdate = result.lrdate;
                        $scope.WorkOrder.invoiceno = result.invoiceno;
                        $scope.WorkOrder.invoicedate = result.invoicedate;
                    }
                });
            }
        }

        $scope.designchanged = function () {
            var design = $scope.DesignList.find(function (item) {
                return item.id == $scope.ItemDetails.designid;
            });
            $scope.ItemDetails.taxid = design.taxid;

            var colourlist = [];
            var sizegrouplist = [];
            $.each(design.sizegroup.items, function (k, v) {
                var exist = colourlist.find(function (item) {
                    return item.id == v.colourid;
                });

                if (typeof (exist) == 'undefined') {
                    colourlist.push({ 'id': v.colourid, 'name': $scope.SearchRef(v.colourid, $scope.AllColours) });
                }

                var exist = sizegrouplist.find(function (item) {
                    return item.sizegroup == v.sizegroup
                });
                if (typeof (exist) == 'undefined' || exist == null) {
                    sizegrouplist.push({ 'sizegroup': v.sizegroup, 'items': [] });
                    sizegrouplist[sizegrouplist.length - 1].items.push(v);
                }
                else {
                    exist.items.push(v);
                }
            });
            $scope.SizeGroupList = sizegrouplist;
            $scope.ColourList = colourlist;
            $timeout(function () {
                if ($scope.mfg_fabricbasedorder)
                    $scope['setFocusinqty']();
                else
                    $scope['setFocussizegroupid']();
            }, 10);

        }

        $scope.AddSetItem = function () {

            var duplicatesno = $scope.WorkOrder.Mfgworkordergrnitems.find(function (item) {
                return item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Please enter different serial number', 'Duplicate serial no');
                return;
            };

            var design = $scope.DesignList.find(function (item) {
                return item.id == $scope.ItemDetails.designid;
            });

            var sizecutratio = [];
            var sleeveid = typeof ($scope.ItemDetails.sleeveid) == 'undefined' ||
                $scope.ItemDetails.sleeveid == null ? 0 : $scope.ItemDetails.sleeveid;
            var colourid = typeof ($scope.ItemDetails.colourid) == 'undefined' ||
                $scope.ItemDetails.colourid == null ? 0 : $scope.ItemDetails.colourid;

            $.each(sizegroup.items, function (k, item) {
                if ((sleeveid == 0 || item.sleeveid == sleeveid) &&
                    (colourid == 0 || item.colourid == colourid)) {
                    sizecutratio.push(item);
                }
            });


            $scope.ItemDetails.productid = design.productid;
            $scope.ItemDetails.productname = $scope.SearchRef(design.productid, $scope.ProductList);
            $scope.ItemDetails.designname = $scope.SearchRef($scope.ItemDetails.designid, $scope.DesignList);
            $scope.ItemDetails.Fabric = $scope.SearchRef($scope.ItemDetails.fabricid, $scope.FabricList);
            $scope.ItemDetails.Colour = $scope.SearchRef($scope.ItemDetails.colourid, $scope.ColourList);
            $scope.ItemDetails.Sleeve = $scope.SearchRef($scope.ItemDetails.sleeveid, $scope.SleeveList);
            $scope.ItemDetails.cutratio = design.cutratio;

            var list = [];
            $.each(sizecutratio, function (k, v) {

                list.push({
                    'sizeid': v.sizeid,
                    'code': v.code,
                    'designid': $scope.ItemDetails.designid,
                    'cutratio': 1,
                    'colourid': $scope.ItemDetails.colourid,
                    'sleeveid': $scope.ItemDetails.sleeveid,
                    'name': $scope.SearchRef(v.sizeid, $scope.Sizes),
                    'sizegroup': sizegroup.sizegroup,
                    'uid': v.uid,
                    'itemid': v.itemid,
                    'qty': parseInt($scope.ItemDetails.sizeqty),
                    'cost': v.cost,
                    'wsrate': v.wsrate,
                    'dealerprice': v.dealerprice,
                    'salerate': v.salerate,
                    'sizeindex': k,
                    'isactive': true,
                    'version': 1
                });

            });

            $scope.ItemDetails.outqty = list.length * parseInt($scope.ItemDetails.sizeqty);
            $scope.ItemDetails.sizelist = list;
            $scope.AddInternalItem();

        }

        $scope.AddInternalItem = function () {
            var internalitem = (JSON.parse(JSON.stringify($scope.ItemDetails)));

            internalitem.amount = $scope.GetNumber(internalitem.cost) * $scope.GetNumber(internalitem.received);

            var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                return (pitem.id == internalitem.taxid);
            });
            internalitem.taxpercentage = 0;
            if (taxitem || taxitem != null) {
                taxitem.taxsplit.findTax = $scope.findTax;
                var taxconfig = taxitem.taxsplit.findTax(internalitem.cost);
                if (taxconfig.cgst == 0) {
                    taxconfig = { 'cgst': 0, 'sgst': 0 };
                }
                internalitem.taxpercentage = taxconfig.cgst + taxconfig.sgst;
            }
            internalitem.taxamount = (internalitem.amount * internalitem.taxpercentage / 100).round(2);
            internalitem.amount += internalitem.taxamount;

            $.each(internalitem.sizelist, function (k, v) {
                var exist = $scope.WorkOrder.sizelist.find(function (item) {
                    return item.sizeid == v.sizeid;
                });

                if (typeof (exist) == 'undefined' || exist == null) {
                    $scope.WorkOrder.sizelist.push({
                        'sizeid': v.sizeid, 'sizeindex': v.sizeindex, 'name': v.name
                    });
                }
            });
            $scope.WorkOrder.sizelist.sort(function (a, b) {
                return a.sizeindex - b.sizeindex;
            });
            $scope.WorkOrder.Mfgworkordergrnitems.push(internalitem);

            $scope.ConfigureGrid(50);
            $scope.ItemDetails.Customize = false;
            $scope.CalculateTotal();
            $scope.ItemDetails.sno = parseInt($scope.WorkOrder.Mfgworkorderitems[$scope.WorkOrder.Mfgworkorderitems.length - 1].sno) + 1;
        }

        $scope.AddItem = function () {

            var duplicatesno = $scope.WorkOrder.Mfgworkordergrnitems.find(function (item) {
                return item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Please enter different serial number', 'Duplicate serial no');
                return;
            };

            var design = $scope.DesignList.find(function (item) {
                return item.id == $scope.ItemDetails.designid;
            });

            var sizecutratio = [];
            $.each(design.sizegroup.items, function (k, item) {
                if (item.sleeveid == $scope.ItemDetails.sleeveid &&
                    item.colourid == $scope.ItemDetails.colourid) {
                    sizecutratio.push(item);
                }
            });

            $scope.ItemDetails.productid = design.productid;
            $scope.ItemDetails.productname = $scope.SearchRef(design.productid, $scope.ProductList);
            $scope.ItemDetails.designname = $scope.SearchRef($scope.ItemDetails.designid, $scope.DesignList);
            $scope.ItemDetails.Fabric = $scope.SearchRef($scope.ItemDetails.fabricid, $scope.FabricList);
            $scope.ItemDetails.Colour = $scope.SearchRef($scope.ItemDetails.colourid, $scope.ColourList);
            $scope.ItemDetails.Sleeve = $scope.SearchRef($scope.ItemDetails.sleeveid, $scope.SleeveList);
            $scope.ItemDetails.cutratio = design.cutratio;


            var list = [];
            var value = $scope.ItemDetails.size;
            var sizeidList = value.toString().split(',');
            var sizelist = sizeidList[0].split('-');
            if (sizelist.length == 2 && /^[a-zA-Z2-6]*$/.test(sizelist[0])) {

                var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                var startsize = sizeIndex.indexOf(sizelist[0]);
                var endsize = sizeIndex.indexOf(sizelist[1]);
                if (startsize < 0 || endsize < 0) {
                    sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                    startsize = sizeIndex.indexOf(sizelist[0]);
                    endsize = sizeIndex.indexOf(sizelist[1]);
                }
                sizelist = [];
                for (var index = startsize; index <= endsize; index++) {
                    sizelist.push(sizeIndex[index]);
                }

            }
            else if (sizelist.length == 3) {

                var firstPart = sizelist[0].match(/[^-_ \d]+|\d+/g);
                var lastPart = sizelist[2].match(/[^-_ \d]+|\d+/g);
                var suffix = '';
                var startsize = parseInt(sizelist[0]);
                var increment = parseInt(sizelist[1]);
                var endsize = parseInt(sizelist[2]);

                if (firstPart.length == 2 && /^[a-zA-Z2-6]*$/.test(firstPart[1]))
                    suffix = firstPart[1];

                sizelist = [];
                for (; startsize <= endsize; startsize = startsize + increment) {
                    var searchsize = startsize.toString() + suffix;

                    var excluded = false;
                    for (var i = 1; i < sizeidList.length; i++) {
                        if (sizeidList[i] == searchsize)
                            excluded = true;
                    }
                    if (!excluded)
                        sizelist.push(searchsize);
                }
            }


            var idList = [];
            $.each($scope.Sizes, function (k, v) {
                $.each(sizelist, function (kk, vv) {
                    if (vv == v.name) {

                        var sizeratio = sizecutratio.find(function (item) {
                            return item.sizeid == v.id;
                        });

                        var cutratio = 1;
                        var code = '';
                        if (typeof (sizeratio) != 'undefined' && sizeratio != null) {
                            cutratio = sizeratio.qty;
                            code = sizeratio.code;
                        }

                        list.push({
                            'sizeid': v.id,
                            'code': code,
                            'designid': $scope.ItemDetails.designid,
                            'cutratio': cutratio,
                            'colourid': $scope.ItemDetails.colourid,
                            'sleeveid': $scope.ItemDetails.sleeveid,
                            'name': v.name,
                            'sizegroup': value,
                            'qty': 0,
                            'cost': 0,
                            'worate': 0,
                            'dealerprice': 0,
                            'salerate': 0,
                            'sizeindex': kk,
                            'isactive': true,
                            'version': 1
                        });
                    }
                });
            });

            list.sort(function (a, b) {
                return a.sizeindex - b.sizeindex;
            });

            $scope.ItemDetails.sizelist = list;
            $scope.ItemDetails.Customize = true;
            $scope.ChangeCR('initialize');
            $scope.ConfigureGrid(50);
        };

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.WorkOrderGRN', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.WorkOrder = value;
        };

        $scope.New = function () {
            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
            $scope.WorkOrder = (JSON.parse(JSON.stringify($scope.WorkOrderTemplate)));
            $scope.WorkOrder.Mfgworkordergrnitems = [];
        }

        $scope.findTax = function (amount) {
            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }


        $scope.SetRowStyle = function (item) {
            if (typeof (item.poinfo) != 'undefined' &&
                item.poinfo != null && item.poinfo.length > 0)
                item.style = { "background-color": "lightgrey" };
            else
                item.style = {};
            return item;

        }

        $scope.RoundPrice = function (amount) {
            return amount.round(0);

            if (amount < 100) {
                amount = Math.round(amount, 0);
            }
            if (amount > 100 && amount < 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    if (diff - 5 >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 10;
                    }
                    else {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                }
                else {
                    if (diff >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                    else {
                        amount = (Math.floor(amount / 10) * 10);
                    }
                }

            } else if (amount > 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    amount = (Math.floor(amount / 10) * 10) + 10;
                }
                else {
                    amount = (Math.floor(amount / 10) * 10);
                }
            }
            return amount;
        }

        $scope.CalculateTotal = function () {

            $scope.WorkOrder.totalqty = 0;
            $scope.WorkOrder.taxable = 0;
            $scope.WorkOrder.tax = 0;
            $scope.WorkOrder.total = 0;

            $.each($scope.currentsizeorders, function (k, v) {
                var qty = $scope.GetNumber(v.qty);
                var cost = parseFloat(v.cost);
                
                if (isNaN(v.qty)) qty = 0.0;
                if (isNaN(v.cost)) cost = 0.0;
                v.taxamount = 0;
                if (v.taxpercentage > 0) {
                    v.taxamount = ((qty * cost) * v.taxpercentage/100).round(2);
                }
                v.amount = v.taxamount + (qty * cost);
                $scope.WorkOrder.totalqty += qty;
                $scope.WorkOrder.tax += v.taxamount;
                $scope.WorkOrder.taxable += (v.amount - v.taxamount);
                $scope.WorkOrder.total += v.amount;
            });

            $scope.WorkOrder.taxable = $scope.WorkOrder.taxable.round(2);
            $scope.WorkOrder.tax = $scope.WorkOrder.tax.round(2);
            $scope.WorkOrder.total = $scope.WorkOrder.total.round(2);
        };


        $scope.InitializeInvoiceList = function (id) {

            request = {
                "Item": { id: id },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "MfgworkordergrnService/Search",
                data: request
            }).success(function (result) {

                $scope.DisplayInvoice(result[0]);

            }).error(function (result) {
            });
        };

        $scope.SaveRequest = false;
        $scope.Save = function (movenext) {

            if (typeof ($scope.WorkOrder.companyid) == 'undefined' || $scope.WorkOrder.companyid == null || $scope.WorkOrder.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select buying company name");
                return;
            }

            var company = $scope.BuyingCompany.find(function (item) { return item.id == $scope.WorkOrder.companyid; });
            if (company == undefined || company == null) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select buying company name");
            }

            if ($scope.WorkOrder.invoicedate == null || $scope.WorkOrder.invoicedate == "") {
                $scope['setFocusinvoicedate']();
                $rootScope.Toaster.Info("Please select Work Order Date");
                return;
            }
            if (typeof ($scope.WorkOrder.receivinglocationid) == 'undefined' || $scope.WorkOrder.receivinglocationid == null || $scope.WorkOrder.receivinglocationid == 0) {
                $scope['setFocusreceivinglocationid']();
                $rootScope.Toaster.Error("Please select the Receiving Location..");
                return false;
            }

            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;
            var request = {
                "Item": $scope.WorkOrder,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgworkordergrnService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("GRN Saved Successfully.!");
                    $scope.WorkOrder.id = result.id;
                    $scope.WorkOrder.version = result.version;
                    $scope.WorkOrder.createdby = result.createdby;
                    $scope.WorkOrder.modifiedby = result.modifiedby;
                    $scope.WorkOrder.createdon = result.createdon;
                    $scope.WorkOrder.modifiedon = result.modifiedon;
                    $scope.WorkOrderItem = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.DisplayInvoice = function (invoice) {
            $scope.Navigation.SetMode('EDIT');
            $scope.ShowSearch = false;


            $timeout(function () {
                $scope.WorkOrder = invoice;
                $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                $scope.CalculateTotal();
                $.each($scope.WorkOrder.Mfgworkordergrnitems, function (k, v) {
                    v.sno = k + 1;
                });
                if ($scope.WorkOrder.Mfgworkordergrnitems.length > 0) {
                    $scope.ItemDetails.sno = $scope.WorkOrder.Mfgworkordergrnitems[$scope.WorkOrder.Mfgworkordergrnitems.length - 1].sno + 1;
                }
                $scope.WorkOrder.sizelist.sort(function (a, b) {
                    return a.sizeindex - b.sizeindex;
                });
                $scope.ConfigureGrid(50);
                $scope.CalculateTotal();
            }, 25);
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $scope.WorkOrder.Mfgworkordergrnitems.splice($scope.WorkOrder.Mfgworkordergrnitems.indexOf(item), 1);
                if ($scope.WorkOrder.Mfgworkordergrnitems.length == 0) {
                    $scope.ItemDetails.sno = 1;
                }
                else {
                    $scope.ItemDetails.sno = $scope.WorkOrder.Mfgworkordergrnitems[$scope.WorkOrder.Mfgworkordergrnitems.length - 1].sno;
                }
                $scope.CalculateTotal();
                //$scope.igrid.UpdateData($scope.WorkOrder.Mfgworkordergrnitems);
            }
            else if (action == "Edit") {
                if (typeof ($scope.ItemDetails.IsEditItem) != 'undefined' &&
                    $scope.ItemDetails.IsEditItem) {
                    $rootScope.Confirm('Editing', 'you will loose the current editing item', function () {
                        $scope.$apply(function () {
                            $scope.EditItem(item);
                            $scope.ItemDetails.IsEditItem = true;
                            //$scope.igrid.UpdateData($scope.WorkOrder.Mfgworkordergrnitems);
                        });
                    });
                }
                else {
                    $scope.EditItem(item);
                    $scope.ItemDetails.IsEditItem = true;
                }

            }
            else if (action == "EditWorkOrder") {
                $scope.InitializeInvoiceList(item.id);
            }
            else if (action == "DeleteWorkOrder") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete the selected purchase entry?',
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
                                    url: "MfgworkordergrnService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.sgrid.UpdateData($scope.SearchResult);
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
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
            }
        }

        $scope.EditItem = function (item) {
            $scope.WorkOrder.Mfgworkordergrnitems.splice($scope.WorkOrder.Mfgworkordergrnitems.indexOf(item), 1);
            $scope.ItemDetails = item;
            $scope.ItemDetails.Customize = true;
            $scope.CalculateTotal();
        }


        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgworkordergrnService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ConfigureSearchGrid();
            }).error(function (result) {
            });
        }

        $scope.ChangeRLocation = function () {
            $scope.WorkOrder.inward2warehouse = true;
            var wl = $scope.WarehouseLocations.find(function (item) {
                return item.id == $scope.WorkOrder.receivinglocationid;
            });

            if (typeof (wl) != 'undefined' && wl != null) {
                $scope.WorkOrder.inward2warehouse = wl.additionalinfo != null && wl.additionalinfo.iswarehouse
            }
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.WorkOrderGRN', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.WorkOrder = value;
        };

        $scope.Print = function (workorderid) {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "workorder.pdf";
                link.href = 'MfgworkordergrnService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (workorderid) != 'undefined' ? workorderid : $scope.WorkOrder.id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.PrintBarcode = function () {
            var result = [];
            var sno = 1;
            $.each(this.egrid.GetSelectedRows(), function (k, v) {

                if ($scope.SizeGrid) {
                    $.each(v.items, function (kk, vv) {
                        var colour = $scope.ColourList.find(function (item) {
                            return item.id == vv.colourid;
                        });
                        var size = $scope.SizeList.find(function (item) {
                            return item.id == vv.sizeid;
                        });

                        result.push({
                            'sno': sno++, 'id': vv.id, 'grnid': $scope.WorkOrder.id, 'designitemcode':
                                vv.designitemcode, 'colour': colour.name, 'uid': vv.uid, 'designid': v.designid,
                            'qty': vv.aqty, 'size': size.name
                        });
                    })
                }
                else {
                    var colour = $scope.ColourList.find(function (item) {
                        return item.id == v.colourid;
                    });
                    var size = $scope.SizeList.find(function (item) {
                        return item.id == v.sizeid;
                    });

                    result.push({
                        'sno': v.sno, 'id': v.id, 'grnid': $scope.WorkOrder.id, 'designitemcode': v.designitemcode,
                        'colour': colour ? colour.name : 'none', 'uid': v.uid, 'designid': v.designid,
                        'qty': v.aqty, 'size': size ?size.name:'','pqty':v.qty
                    }); 
                }
            });
            $scope.ChooseBarcode(result);
        }

        $scope.ShowStatus = function () {
            var dialog = ngDialog.open({
                template: 'ChooseStatus',
                data: {
                    'parentscope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.StatusList = [
                        { 'id': 1, 'name': 'Opening' },
                        { 'id': 2, 'name': 'Barcoding' },
                        { 'id': 3, 'name': 'Racking' }];
                    $scope.Employee = $scope.ngDialogData.parentscope.Employee;
                    $scope.Selected = function () {
                        if ($scope.statusid && $scope.statusid != null &&
                            $scope.statusid != '') {
                            $scope.closeThisDialog(
                                {
                                    'status': $scope.statusid,
                                    'employeeid': $scope.employeeid,
                                    'actionon': $scope.entrydate,
                                });
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {
                        $scope.statuschanged(data.value);
                    }
                }
            });
        }

        $scope.statuschanged = function (status) {
            var list = [];
            $.each($scope.sgrid.GetSelectedRows(), function (k, v) {
                list.push(v.id);
            });
            request = {
                "Item": {
                    'ids': list, 'status': status.status,
                    'employeeid': status.employeeid,
                    'actionon': status.actionon
                },
                "SecurityToken": "String content"
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgworkordergrnService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $scope.Search();
            });
        };

        $scope.ChooseBarcode = function (barcodeList) {

            var dialog = ngDialog.open({
                template: 'ChooseBarecodes',
                data: {
                    'restService': $scope.restService,
                    'currentlist': barcodeList
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.restService = $scope.ngDialogData.restService;
                    $scope.currentlist = $scope.ngDialogData.currentlist;
                    $scope.SelectAll = function (value) {
                        $.each($scope.currentlist, function (k, v) {
                            v.selected = value;
                        })
                    }

                    $scope.Selected = function () {
                        var returnList = []
                        $.each($scope.currentlist, function (k, v) {
                            if (v.selected) returnList.push(v);
                        });
                        $scope.closeThisDialog(returnList);
                    }

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        if (data.value.length > 0) {

                            var requestitem = { 'items': data.value };
                            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
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
                                    url: "MfgworkordergrnService/PrintBarcode",
                                    data: srequest
                                }).success(function (result) {
                                    if (result != null && result.length > 0) {
                                        $rootScope.Sent2Printer(result.join(';'), printdata);
                                    }
                                });
                            });

                        }
                    }
                }
            });
        }

        if (typeof ($location.search().id) != 'undefined' &&
            $location.search().id.length > 0) {
            request = {
                "Item": {
                    'workorderid': $location.search().id,
                    'creategrn': true
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgworkordergrnService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowSearch = true;
                if (result.length == 1) {
                    $scope.DisplayInvoice(result[0]);
                }
            }).error(function (result) {
            });
        }

    }]);
