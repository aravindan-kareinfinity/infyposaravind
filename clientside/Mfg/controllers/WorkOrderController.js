var app = angular.module('InfinityLab');

app.controller('WorkOrderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('EDIT');
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = false;
        $scope.SaveRequest = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = '';
        $scope.WorkOrderTaxItem = {};
        $scope.additionaldiscountpercentage = 0;
        $scope.mfg_fabricbasedorder = true;
        $scope.mfg_designclass = "col-md-3";
        $scope.SizeGroupList = [];
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }
        $scope.ShowTax = function () {
            $scope.showtax = !$scope.showtax;
        }

        $scope.SearchCondition = {
            companyid: 0,
            supplierid: 0,
            invoiceno: "",
            addedfrom: null,
            addedto: null,
            lrentryno: ""
        }

        $scope.ReportList = [];
        restService.Get('ConfigurationService/ReportList?id=WORKORDER', true).then(function (results) {
            $scope.ReportList = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.WorkflowList = restService.Get('ReferencelistService/MFGWOFLOW', true).then(function (results) {
            $scope.WorkflowList = results;
        });

        $scope.SizeGroups = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroups = results;
        });

        $scope.WarehouseLocations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            if (results.length == 0)
                results.push({ 'id': 0, 'name': 'none' });
            $scope.AllSleeveList = results;
            $scope.SleeveList = results;
        });

        $scope.AllColours = [];
        $scope.ColourList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColourList = results;
            $scope.AllColours = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
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

            $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
                $.each(results, function (k, v) {

                    var taxitem = $scope.purchaseTaxActual.find(function (item) {
                        return item.id == v.referenceid;
                    });

                    try {
                        var taxitem = (JSON.parse(JSON.stringify(taxitem)));
                        taxitem.name = v.name + ' @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = 'Ad_' + taxitem.id + '_' + 1;
                        if (taxitem.taxpercentage > 0)
                            taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage / 2, 'sgst': taxitem.taxpercentage / 2 }] };
                        else
                            taxitem.taxsplit = { 'items': [{ 'cgst': 0, 'sgst': 0 }] };
                        taxitem.type = 8;
                        $scope.PurchaseTax.push(taxitem);
                    } catch (x) {

                    }
                });

            });
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });
        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });

        $scope.FabricList = restService.Get('ReferencelistService/FABRIC', true).then(function (results) {
            $scope.FabricList = results;
            if ($scope.WorkOrder && $scope.WorkOrder.onlybom && !$scope.IsSampleBased) {
                $timeout(function () {
                    if ($scope.WorkOrder.onlybom)
                        $scope.ConfigureBOMGrid(100);
                    else
                        $scope.ConfigureGrid(100);
                }, 25);
            }
        });

        $scope.StatusList = [
            { id: 1, name: 'Created' },
            { id: 2, name: 'Outward' },
            { id: 3, name: 'Progress' },
            { id: 8, name: 'Cancelled' },
            { id: 9, name: 'Completed' }];

        $scope.ItemTemplate = {
            "taxid": null,
            "readonly": false,
            "isactive": true,
            "version": 1,
            "designid": "",
            "sleeveid": 0,
            "inqty": "",
            "length": "",
            "cutratio": "",
            "cost": "",
            "sizeqty": "",
            "sno": 1
        }

        $scope.WorkOrderTemplate = {
            "companyid": 0, "mode": "Internal", "workorderno": "",
            "id": 0, "modifiedon": "", "createdon": "",
            "createdby": 0, "modifiedby": 0, "version": 1,
            "suppliercompanyid": 0, "isactive": true,
            "wodate": new Date(),
            "expectdate": new Date(),
            "onlybom": false,
            Mfgworkorderitems: [],
            sizelist: [],
            remarks: ""
        }

        $scope.LastPrice = null;

        $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
        $scope.WorkOrder = (JSON.parse(JSON.stringify($scope.WorkOrderTemplate)));
        $scope.FromColourList = [];
        $scope.ToColourList = [];
        $scope.BOMFabricChanged = function () {
            var fromfabric = $scope.FabricList.find(function (item) {
                return item.id == $scope.ItemDetails.fabricid;
            });
            if (fromfabric && fromfabric != null) {
                var colourlist = []
                $.each(fromfabric.colourlist, function (k, v) {
                    colourlist.push({ 'id': v.id, 'name': v.name + '-' + $scope.SearchRef(v.id, $scope.AllColours) });
                })
                $scope.FromColourList = colourlist;
            }
        }

        $scope.BOMToFabricChanged = function () {
            var fromfabric = $scope.FabricList.find(function (item) {
                return item.id == $scope.ItemDetails.efabricid;
            });
            if (fromfabric && fromfabric != null) {
                var colourlist = []
                $.each(fromfabric.colourlist, function (k, v) {
                    colourlist.push({ 'id': v.id, 'name': v.name + '-' + $scope.SearchRef(v.id, $scope.AllColours) });
                })
                $scope.ToColourList = colourlist;
            }
        }

        $scope.fabricChange = function (callback) {

            if ($scope.ItemDetails.fabricid && $scope.ItemDetails.fabricid != null && $scope.ItemDetails.fabricid != '') {
                restService.Get('MfgdesignService/SearchByFabricDesign/' + $scope.ItemDetails.fabricid, true).then(function (results) {
                    $scope.DesignList = results;
                    if (callback) {
                        callback();
                    }
                    else {
                        $timeout(function () {
                            if ($scope.mfg_fabricbasedorder)
                                $scope.setFocusdeisgnid();
                            else
                                $scope.setFocussizegroupid();
                        }, 10);
                    }
                });
            }
        }

        $scope.WorkflowChanged = function () {
            $scope.WorkOrder.onlybom = false;
            var workflow = $scope.WorkflowList.find(function (item) {
                return item.id == $scope.WorkOrder.woflowid;
            });
            if (workflow && workflow.fgsupport && workflow.fgsupport == 'No' && !$scope.IsSampleBased) {
                $scope.WorkOrder.onlybom = true;
            }

            if ($scope.WorkOrder.onlybom) {
                $scope.ConfigureBOMGrid($scope.mfg_fabricbasedorder ? 200 : 100);
            }
            else {
                $scope.ConfigureGrid($scope.mfg_fabricbasedorder ? 200 : 100);
            }
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

        $scope.SupplierChanged = function () {
            if (typeof ($scope.WorkOrder.suppliercompanyid) != 'undefined' ||
                $scope.WorkOrder.suppliercompanyid != null) {
                $scope.WorkOrder.supplierid = $scope.SupplierCompany.find(function (item) {
                    return item.id == $scope.WorkOrder.suppliercompanyid;
                }).id;
            }
        }

        $scope.ChangeCR = function (type) {
            if (!$scope.mfg_fabricbasedorder) return;

            var inqty = $scope.GetNumber($scope.ItemDetails.inqty);
            var cutratio = $scope.GetNumber($scope.ItemDetails.cutratio);
            if (type == 'cutratio' || type == 'initialize' || type == 'inqty') {
                $scope.ItemDetails.outqty = Math.floor(inqty / cutratio);
            }

            if (type == 'inqty') {
                $scope.NegativeStock = $scope.mfg_fabricwostock;
                if ($scope.NegativeStock) $scope.ValidateFabric();
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
                $scope.ItemDetails.outqty = allqty;
                $scope.ItemDetails.cutratio = (inqty / allqty).toFixed(2);
            }
            else {
                var maxqty = 0;
                var maxindex = -1;
                $.each($scope.ItemDetails.sizelist, function (k, v) {
                    var cutratio = $scope.GetNumber(v.cutratio);
                    if (!isNaN(cutratio)) {
                        v.qty = Math.round(cutratio / totqty * $scope.ItemDetails.outqty);
                        allqty += v.qty;
                        if (k == 0 || v.qty > maxqty) {
                            maxindex = k;
                            maxqty = v.qty;
                        }
                    }
                });
                if (allqty != $scope.ItemDetails.outqty) {
                    if ($scope.ItemDetails.outqty > allqty) {
                        $scope.ItemDetails.sizelist[0].qty += ($scope.ItemDetails.outqty - allqty);
                    }
                    else if (maxindex >= 0) {
                        $scope.ItemDetails.sizelist[maxindex].qty -= (allqty - $scope.ItemDetails.outqty);
                    }
                }
            }
        }

        $scope.currentdesign = null;
        $scope.designChanged = function (focus, isedit, callback) {
            isedit = typeof (isedit) == 'undefined' ? false : true;
            focus = typeof (focus) == 'undefined' ? true : focus;
            var designs = $scope.DesignList.find(function (item) {
                return item.id == $scope.ItemDetails.designid;
            });

            if (typeof (designs) != 'undefined' || designs != null) {
                restService.Get('MfgdesignService/Search/' + $scope.ItemDetails.designid, true).then(function (results) {

                    if (results != null) {
                        var design = results;
                        $scope.currentdesign = design;
                        designs.sizegroup = results.sizegroup;
                        if (!isedit) {
                            $scope.ItemDetails.cutratio = design.cutratio;
                            if (design.sizegroup.items[0].cost > 0)
                                $scope.ItemDetails.cost = design.sizegroup.items[0].cost;
                            else if (design.sizegroup.items[0].wsrate > 0)
                                $scope.ItemDetails.cost = design.sizegroup.items[0].wsrate;
                            if ($scope.ItemDetails.cutratio == 0)
                                $scope.ItemDetails.cutratio = '';
                            if ($scope.ItemDetails.cost == 0)
                                $scope.ItemDetails.cost = '';
                        }
                        //$scope.ColourList
                        var colourlist = [];
                        var colourdesignlist = [];
                        var sleevelist = [];
                        var sizegrouplist = [];

                        $.each(design.sizegroup.items, function (k, v) {
                            var exist = colourdesignlist.find(function (item) {
                                return item.colourid == v.colourid && item.code == v.code;
                            });

                            if (typeof (exist) == 'undefined') {
                                colourdesignlist.push({
                                    'colourid': v.colourid,
                                    'id': v.colourid,
                                    'name': $scope.SearchRef(v.colourid, $scope.AllColours),
                                    'code': v.code,
                                    'dname': v.code,
                                    'display': v.code + '-' + $scope.SearchRef(v.colourid, $scope.AllColours),
                                    'items': []
                                });
                                var exist = colourdesignlist.find(function (item) {
                                    return item.colourid == v.colourid && item.code == v.code;
                                });
                            }

                            exist.items.push(v);

                            var exist = colourlist.find(function (item) {
                                return item.id == v.colourid;
                            });

                            if (typeof (exist) == 'undefined') {
                                colourlist.push({
                                    'id': v.colourid,
                                    'colourid': v.colourid,
                                    'dname': v.code,
                                    'code': v.code,
                                    'name': $scope.SearchRef(v.colourid, $scope.AllColours),
                                    'display': v.code + '-' + $scope.SearchRef(v.colourid, $scope.AllColours)
                                });
                            }

                            var exist = sleevelist.find(function (item) {
                                return item.id == v.sleeveid;
                            });

                            if (typeof (exist) == 'undefined') {
                                sleevelist.push({ 'id': v.sleeveid, 'name': $scope.SearchRef(v.sleeveid, $scope.AllSleeveList) });
                            }

                            var exist = sizegrouplist.find(function (item) {
                                return item.sizegroup == v.sizegroup
                            });
                            if (typeof (exist) == 'undefined' || exist == null) {
                                $scope.ItemDetails.size = v.sizegroup;
                                sizegrouplist.push({ 'sizegroup': v.sizegroup, 'items': [] });
                                sizegrouplist[sizegrouplist.length - 1].items.push(v);
                            }
                            else {
                                exist.items.push(v);
                            }
                        });
                        $scope.SizeGroupList = sizegrouplist;
                        if (colourdesignlist.length == colourlist.length) {
                            $scope.ColourList = colourlist;
                            if (isedit) {
                                var ecolour = $scope.ItemDetails.colourid;
                                $scope.ItemDetails.colourid = null;
                                $timeout(function () {
                                    $scope.ItemDetails.colourid = ecolour;
                                }, 10);
                            }
                        }
                        else {
                            $scope.ColourList = colourdesignlist;
                            if (isedit) {
                                var ecolour = colourdesignlist.find(function (item) {
                                    return item.dname == $scope.ItemDetails.designitemname;
                                });
                                if (ecolour && ecolour != null) {
                                    $scope.ItemDetails.colourid = null;
                                    $timeout(function () {
                                        $scope.ItemDetails.colourid = ecolour.id;
                                    }, 10);
                                }
                            }
                        }
                        $scope.SleeveList = sleevelist;
                        if (typeof (focus) == 'undefined' || focus) {
                            $timeout(function () {
                                if ($scope.mfg_fabricbasedorder)
                                    $scope['setFocusinqty']();
                                else
                                    $scope['setFocussizegroupid']();
                            }, 10);
                        } else if (callback) {
                            callback()
                        }

                    }
                });
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

        $scope.ConfigureBOMGrid = function (additionalheight) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, additionalheight);

            var commands = "";
            if ($scope.mfg_fabricbasedorder)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>&nbsp;"

            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>&nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete') {
                        $scope.ManageSearchItem('Delete', args.item);
                    }
                });
            });

            igrid.AddSimpleColumn('sno', 'S.No', 80);
            igrid.AddReferenceColumn('fabricid', 'From Fabric', 100, $scope.FabricList);
            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColours);
            igrid.AddReferenceColumn('efabricid', 'To Fabric', 100, $scope.FabricList);
            igrid.AddReferenceColumn('efabriccolourid', 'Colour', 100, $scope.AllColours);
            igrid.AddFormatedColumn('outqty', 'Issuing Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('inqty', 'Receiving Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cost', 'Cost', 80, 'N2', 2);
            igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');


        }

        $scope.ConfigureGrid = function (additionalheight) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, additionalheight);

            var commands = "";
            if ($scope.mfg_fabricbasedorder)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>&nbsp;"

            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>&nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete') {
                        $scope.ManageSearchItem('Delete', args.item);
                    }

                });
            });

            igrid.AddSimpleColumn('sno', 'S.No', 50);
            igrid.AddSimpleColumn('designname', 'Design', 80);
            igrid.AddSimpleColumn('designitemname', 'Code', 80);
            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColours);

            var sizeWidth = 50;
            if (!$scope.IsSampleBased) {
                igrid.AddReferenceColumn('sleeveid', 'Sleeve', 100, $scope.SleeveList);
                if ($scope.mfg_fabricbasedorder) {
                    igrid.AddFormatedColumn('inqty', 'In Qty', 60, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('cutratio', 'Cut Ratio', 60, 'N2', 2);
                    igrid.AddFormatedColumn('length', 'Length', 60, 'N2', 2);
                }
                igrid.AddFormatedColumn('outqty', 'Out Qty', 60, 'N2', 2).total = true;
            }
            else {
                var sizeWidth = 100;
                $scope.WorkOrder.sizelist = [];
                $.each($scope.WorkOrder.Mfgworkorderitems, function (k, v) {
                    $.each(v.sizelist, function (kk, vv) {
                        var size = $scope.WorkOrder.sizelist.find(function (item) {
                            return item.sizeid == vv.sizeid;
                        });
                        if (size == undefined || size != null) {
                            $scope.WorkOrder.sizelist.push({
                                'sizeid': vv.sizeid,
                                'name': $scope.SearchRef(vv.sizeid, $scope.Sizes)
                            })
                        }
                    })
                });
            }

            $.each($scope.WorkOrder.sizelist, function (k, v) {

                var column = igrid.AddColumn(
                    {
                        id: 'qty_' + v.sizeid, name: v.name, field: 'qty_' + v.sizeid,
                        width: sizeWidth, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var value = item.sizelist.find(function (item) {
                        return item.sizeid == col.internal.sizeid;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return 0;

                    return value.qty;
                };
                column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                    var sizeid = columnDef.internal.sizeid;
                    var value = dataContext.sizelist.find(function (item) {
                        return item.sizeid == sizeid;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return 0;

                    return value.qty.toString();
                }
            });


            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var sizeid = args.grid.getColumns()[args.cell].internal.sizeid;
                var editcellid = args.grid.getColumns()[args.cell].id;
                var sizeitem = item.sizelist.find(function (sitem) {
                    return sitem.sizeid == sizeid
                });

                if (sizeitem && sizeitem != null) {
                    item.outqty -= sizeitem.qty;
                    sizeitem.qty = parseFloat(item[editcellid]);
                    item.outqty += sizeitem.qty;
                } else {
                    item.sizelist.push({ 'sizeid': sizeid, 'qty': parseFloat(item[editcellid]) });
                }

                $timeout(function () {
                    args.grid.updateRow(args.row);
                    $scope.igrid.UpdateAllTotals(args.grid);
                    $scope.CalculateTotal();
                }, 10);
            });


            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');
        }

        $scope.AddBOMItem = function () {
            if ($scope.ItemDetails.id && $scope.ItemDetails.id > 0) {
                $scope.ItemDetails.Customize = false;
                $scope.ItemDetails.IsEditItem = false;
                $scope.CalculateTotal();
                $scope.WorkOrder.Mfgworkorderitems.push($scope.ItemDetails);
                $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');
                $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                $scope.ItemDetails.id = 0;
            }
            else {
                var internalitem = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                internalitem.cost = $scope.GetNumber(internalitem.cost);
                internalitem.outqty = $scope.GetNumber(internalitem.outqty);
                internalitem.inqty = $scope.GetNumber(internalitem.inqty);
                if (isNaN(internalitem.cost)) internalitem.cost = 0;
                if (isNaN(internalitem.outqty)) internalitem.outqty = 0;
                if (isNaN(internalitem.inqty)) internalitem.inqty = 0;
                internalitem.amount = $scope.GetNumber(internalitem.cost) * $scope.GetNumber(internalitem.inqty);

                internalitem.taxid = $scope.FabricList.find(function (item) {
                    return item.id == $scope.ItemDetails.efabricid;
                }).taxid;

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
                internalitem.isactive = true;
                internalitem.version = 1;
                $scope.WorkOrder.Mfgworkorderitems.push(internalitem);
                $scope.ConfigureBOMGrid($scope.mfg_fabricbasedorder ? 200 : 100);
                $scope.ItemDetails.Customize = false;
                $scope.CalculateTotal();
                $scope.ItemDetails.sno = parseInt($scope.WorkOrder.Mfgworkorderitems[$scope.WorkOrder.Mfgworkorderitems.length - 1].sno) + 1;
            }
            $scope.setFocusbomsno();
        }




        $scope.AddInternalItem = function () {

            if ($scope.ItemDetails.id && $scope.ItemDetails.id > 0) {
                $scope.ItemDetails.Customize = false;
                $scope.ItemDetails.IsEditItem = false;
                $scope.CalculateTotal();
                $scope.WorkOrder.Mfgworkorderitems.push($scope.ItemDetails);
                $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');
                $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                $scope.ItemDetails.id = 0;
            }
            else {
                var internalitem = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                internalitem.cost = $scope.GetNumber(internalitem.cost);
                internalitem.outqty = $scope.GetNumber(internalitem.outqty);
                if (isNaN(internalitem.cost)) internalitem.cost = 0;
                if (isNaN(internalitem.outqty)) internalitem.outqty = 0;
                internalitem.amount = $scope.GetNumber(internalitem.cost) * $scope.GetNumber(internalitem.outqty);

                internalitem.taxid = $scope.DesignList.find(function (item) {
                    return item.id == $scope.ItemDetails.designid;
                }).taxid;

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
                internalitem.sizecount = internalitem.sizelist.length;
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
                internalitem.isactive = true;
                internalitem.version = 1;
                $scope.WorkOrder.Mfgworkorderitems.push(internalitem);

                $scope.ConfigureGrid(100);
                $scope.ItemDetails.Customize = false;
                $scope.CalculateTotal();
                $scope.ItemDetails.sno = parseInt($scope.WorkOrder.Mfgworkorderitems[$scope.WorkOrder.Mfgworkorderitems.length - 1].sno) + 1;
            }
            $scope.setFocussno();
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

        $scope.AddSizeGroupItem = function () {
            var duplicatesno = $scope.WorkOrder.Mfgworkorderitems.find(function (item) {
                return item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Please enter different serial number', 'Duplicate serial no');
                return;
            };

            var design = $scope.DesignList.find(function (item) {
                return item.id == $scope.ItemDetails.designid;
            });

            var sizegroup = $scope.SizeGroups.find(function (item) {
                return item.id == $scope.ItemDetails.sizegroupid;
            });


            var sizecutratio = [];

            $.each(design.sizegroup.items, function (k, v) {
                if ($scope.ItemDetails.designitemname == v.code) {
                    sizecutratio.push(v);
                }
            });


            if (!$scope.mfg_fabricbasedorder && sizegroup && sizegroup != null && sizegroup.list != null) {
                var fsizecutratio = [];
                $.each(sizecutratio, function (k, v) {
                    var existsize = sizegroup.list.find(function (item) {
                        return item.id == v.sizeid;
                    });
                    if (existsize && existsize != null) {
                        fsizecutratio.push(v);
                    }
                })
                if (fsizecutratio.length > 0)
                    sizecutratio = fsizecutratio;
            }

            var colouritem = $scope.ColourList.find(function (item) {
                return item.dname == $scope.ItemDetails.designitemname;
            });

            $scope.ItemDetails.colourid = colouritem.colourid;

            var sleeveid = typeof ($scope.ItemDetails.sleeveid) == 'undefined' ||
                $scope.ItemDetails.sleeveid == null ? 0 : $scope.ItemDetails.sleeveid;
            var colourid = typeof ($scope.ItemDetails.colourid) == 'undefined' ||
                $scope.ItemDetails.colourid == null ? 0 : $scope.ItemDetails.colourid;

            $scope.ItemDetails.size = $scope.ItemDetails.sizegroupid;
            $scope.ItemDetails.productid = design.productid;
            $scope.ItemDetails.productname = $scope.SearchRef(design.productid, $scope.ProductList);
            $scope.ItemDetails.designname = $scope.SearchRef($scope.ItemDetails.designid, $scope.DesignList);

            if (!$scope.mfg_fabricbasedorder) {
                $scope.ItemDetails.fabricid = design.fabricid;
            }

            $scope.ItemDetails.Fabric = $scope.SearchRef($scope.ItemDetails.fabricid, $scope.FabricList);
            $scope.ItemDetails.Colour = $scope.SearchRef($scope.ItemDetails.colourid, $scope.AllColours);
            $scope.ItemDetails.Sleeve = $scope.SearchRef($scope.ItemDetails.sleeveid, $scope.SleeveList);
            $scope.ItemDetails.cutratio = design.cutratio;
            var list = [];
            var totalcutratio = 0;
            var totalqty = 0;
            $scope.ItemDetails.outqty = parseInt($scope.ItemDetails.sizeqty);
            if (isNaN($scope.ItemDetails.outqty)) {
                $scope.ItemDetails.outqty = 1;
                $scope.ItemDetails.sizeqty = 1;
            }
            $.each(sizecutratio, function (k, v) {
                totalcutratio += v.qty;
            })
            if (totalcutratio == 0) {
                $.each(sizecutratio, function (k, v) {
                    v.qty = 1;
                    totalcutratio += v.qty;
                })
            }
            $.each(sizecutratio, function (k, v) {
                var selectedsize = $scope.Sizes.find(x => x.id == v.sizeid);
                var sortingindex = k;
                if (selectedsize && selectedsize != null)
                    sortingindex = $scope.Sizes.indexOf(selectedsize);
                var itm = {
                    'sizeid': v.sizeid,
                    'code': v.code,
                    'designid': $scope.ItemDetails.designid,
                    'cutratio': 1,
                    'colourid': $scope.ItemDetails.colourid,
                    'sleeveid': $scope.ItemDetails.sleeveid,
                    'name': $scope.SearchRef(v.sizeid, $scope.Sizes),
                    'sizegroup': sizegroup && sizegroup != null ? sizegroup.name : '-',
                    'uid': v.uid,
                    'itemid': v.itemid,
                    'qty': parseInt(parseInt($scope.ItemDetails.sizeqty) / totalcutratio * v.qty),
                    'cost': v.cost,
                    'wsrate': v.wsrate,
                    'dealerprice': v.dealerprice,
                    'salerate': v.salerate,
                    'sizeindex': sortingindex,
                    'isactive': true,
                    'version': 1
                };
                if (itm.qty < 0 || totalqty > $scope.ItemDetails.outqty)
                    itm.qty = 0;
                list.push(itm);
                totalqty += parseInt(parseInt($scope.ItemDetails.sizeqty) / totalcutratio * v.qty);
                if (totalqty > $scope.ItemDetails.sizeqty) v.qty -= ($scope.ItemDetails.sizeqty - totalqty);
            });


            if ($scope.ItemDetails.outqty != totalqty) {
                for (var i = 0; i < list.length && totalqty != $scope.ItemDetails.outqty; i++) {
                    if ($scope.ItemDetails.outqty > totalqty) {
                        list[i].qty = list[i].qty + 1;
                        totalqty++;
                    }
                    else {
                        list[i].qty = list[i].qty - 1;
                        totalqty--;
                    }
                }
                if (totalqty != $scope.ItemDetails.outqty) {
                    if (totalqty > $scope.ItemDetails.outqty) {
                        list[0].qty -= (totalqty - $scope.ItemDetails.outqty);
                    }
                    else {
                        list[0].qty += ($scope.ItemDetails.outqty - totalqty);
                    }
                }
            }
            $scope.ItemDetails.sizelist = list;
            $scope.AddInternalItem();
        }

        $scope.AddItem = function () {

            var duplicatesno = $scope.WorkOrder.Mfgworkorderitems.find(function (item) {
                return item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Duplicate serial no', 'Please enter different serial number');
                return;
            };

            if ((typeof ($scope.WorkOrder.companyid) == 'undefined' || $scope.WorkOrder.companyid == null ||
                $scope.WorkOrder.companyid == '') ||
                $scope.WorkOrder.companyid <= 0) {
                $rootScope.ShowAlert('Validation', 'Please select the company');
                return;
            }

            if ((typeof ($scope.WorkOrder.locationid) == 'undefined' || $scope.WorkOrder.locationid == null ||
                $scope.WorkOrder.locationid == '') ||
                $scope.WorkOrder.locationid <= 0) {
                $rootScope.ShowAlert('Validation', 'Please select the location');
                return;
            }
            var design = $scope.currentdesign;
            /*$scope.DesignList.find(function (item) {
                return item.id == $scope.ItemDetails.designid;
            });*/

            var colouritem = $scope.ColourList.find(function (item) {
                return item.dname == $scope.ItemDetails.designitemname;
            });

            $scope.ItemDetails.colourid = colouritem.colourid;
            //$scope.ItemDetails.designitemname = colouritem.code;
            var sizecutratio = [];
            $.each(design.sizegroup.items, function (k, item) {
                if (item.code == $scope.ItemDetails.designitemname) {
                    sizecutratio.push(item);
                }
            });

            if (sizecutratio.length == 0) {
                $.each(design.sizegroup.items, function (k, item) {
                    var existsize = sizecutratio.find(function (fitem) {
                        return fitem.sizeid == item.sizeid;
                    });
                    if (typeof (existsize) == 'undefined' || existsize == null) {
                        if (item.colourid == $scope.ItemDetails.colourid) {
                            sizecutratio.push(item);
                        }
                    }
                });
            }

            $scope.ItemDetails.productid = design.productid;
            $scope.ItemDetails.productname = $scope.SearchRef(design.productid, $scope.ProductList);
            $scope.ItemDetails.designname = $scope.SearchRef($scope.ItemDetails.designid, $scope.DesignList);
            $scope.ItemDetails.Fabric = $scope.SearchRef($scope.ItemDetails.fabricid, $scope.FabricList);
            $scope.ItemDetails.Colour = $scope.SearchRef($scope.ItemDetails.colourid, $scope.AllColours);
            $scope.ItemDetails.Sleeve = $scope.SearchRef($scope.ItemDetails.sleeveid, $scope.SleeveList);

            if (design.cutratio > 0 && $scope.ItemDetails.cutratio == 0)
                $scope.ItemDetails.cutratio = design.cutratio;
            var list = [];
            if (typeof ($scope.ItemDetails.size) == 'undefined' || $scope.ItemDetails.size == null) $scope.ItemDetails.size = '';
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
            else {
                var singlesize = $scope.Sizes.find(function (item) {
                    return item.name == value;
                });
                if (typeof (singlesize) == 'undefined') {
                    $.each(sizecutratio, function (k, v) {
                        var size = $scope.Sizes.find(function (sitem) {
                            return sitem.id == v.sizeid;
                        });
                        if (sizelist.indexOf(size.name) < 0)
                            sizelist.push(size.name);
                    })
                }
            }


            var idList = [];
            $.each($scope.Sizes, function (k, v) {
                $.each(sizelist, function (kk, vv) {
                    if (vv == v.name) {

                        var sizeratio = sizecutratio.find(function (item) {
                            return item.sizeid == v.id || item.code == v.name;
                        });

                        //if (typeof (sizeratio) == 'undefined' && design.sizegroup.items != null) {
                        //    sizeratio = design.sizegroup.items.find(function (item) {
                        //        return item.sizeid == v.id || item.code == v.name;
                        //    });
                        //}

                        var cutratio = 1;
                        var code = v.name;
                        var itemid = '';
                        var uid = '';
                        var cost = 0;
                        var wsrate = 0;
                        var dealerprice = 0;
                        var salerate = 0;
                        if (typeof (sizeratio) != 'undefined' && sizeratio != null) {
                            cutratio = sizeratio.qty;
                            if (sizeratio.code != null && sizeratio.code != '')
                                code = sizeratio.code;
                            cost = sizeratio.cost;
                            wsrate = sizeratio.wsrate;
                            dealerprice = sizeratio.dealerprice;
                            salerate = sizeratio.salerate;
                            itemid = sizeratio.itemid;
                            uid = sizeratio.uid;
                        }

                        if (cutratio == '' || cutratio == null || cutratio == '')
                            cutratio = 1;
                        if (typeof (uid) != 'undefined' && uid != '' && uid != null && uid != 'null') {
                            list.push({
                                'sizeid': v.id,
                                'code': code,
                                'designid': $scope.ItemDetails.designid,
                                'cutratio': cutratio,
                                'colourid': $scope.ItemDetails.colourid,
                                'sleeveid': $scope.ItemDetails.sleeveid,
                                'name': v.name,
                                'sizegroup': value,
                                'uid': uid,
                                'itemid': itemid,
                                'qty': 0,
                                'cost': cost,
                                'wsrate': wsrate,
                                'dealerprice': dealerprice,
                                'salerate': salerate,
                                'sizeindex': kk,
                                'isactive': true,
                                'version': 1
                            });
                        }
                    }
                });
            });

            list.sort(function (a, b) {
                return a.sizeindex - b.sizeindex;
            });

            $scope.ItemDetails.sizelist = list;
            $scope.ItemDetails.Customize = true;
            $scope.ChangeCR('initialize');
            $scope.ConfigureGrid(200);
            $scope.NegativeStock = $scope.mfg_fabricwostock;
            if ($scope.NegativeStock) $scope.ValidateFabric();
        };

        $scope.ValidateFabric = function () {

            if ($scope.NegativeStock) {
                var request = {
                    "Item": {
                        'mode': 3,
                        'bomid': $scope.ItemDetails.fabricid,
                        'colourid': $scope.ItemDetails.colourid,
                        'locationid': $scope.WorkOrder.locationid,
                        'companyid': $scope.WorkOrder.companyid,
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BomstocktransactionService/StockReport",
                    data: srequest
                }).success(function (result) {
                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info('Zero Stock Available');
                    } else {
                        if (result[0].closing < $scope.ItemDetails.inqty) {
                            $rootScope.Toaster.Info(result[0].closing + ' Stock Only Available');
                        }
                        else {
                            $scope.NegativeStock = false;
                        }
                    }
                });
            }
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.WorkOrder', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.WorkOrder = value;
        };

        $scope.New = function () {
            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
            $scope.WorkOrder = (JSON.parse(JSON.stringify($scope.WorkOrderTemplate)));
            $scope.WorkOrder.Mfgworkorderitems = [];
            $scope.WorkOrder.Lrentrydetails = [];
            $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');

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

            $scope.WorkOrder.taxable = 0;
            $scope.WorkOrder.taxamount = 0;
            $scope.WorkOrder.total = 0;
            $scope.WorkOrder.totalqty = 0;
            $scope.WorkOrder.totalinqty = 0;

            $.each($scope.WorkOrder.Mfgworkorderitems, function (k, v) {

                v.outqty = parseFloat(v.outqty);
                v.cost = parseFloat(v.cost);
                if (isNaN(v.outqty)) v.outqty = 0.0;
                if (isNaN(v.inqty)) v.inqty = 0.0;
                if (isNaN(v.cost)) v.cost = 0.0;
                if (isNaN(v.taxamount)) v.taxamount = 0.0;

                if (v.sizelist && v.sizelist != null) {
                    v.amount = 0;
                    //v.cost = 0;
                    //v.inqty = 0;
                    v.outqty = 0;
                    v.taxamount = 0;
                    $.each(v.sizelist, function (kk, vv) {
                       // v.inqty += parseFloat(vv.qty);
                        v.outqty += parseFloat(vv.qty);
                        //v.cost += parseFloat(vv.cost) * parseFloat(vv.qty);
                        v.amount += parseFloat(vv.cost) * parseFloat(vv.qty);
                    });
                }

                $scope.WorkOrder.totalinqty += v.inqty;
                $scope.WorkOrder.totalqty += v.outqty;
                $scope.WorkOrder.taxable += v.amount - v.taxamount;
                $scope.WorkOrder.taxamount += v.taxamount;
                $scope.WorkOrder.total += v.amount;
            });

            $scope.WorkOrder.taxable = $scope.WorkOrder.taxable.round(2);
            $scope.WorkOrder.taxamount = $scope.WorkOrder.taxamount.round(2);
            $scope.WorkOrder.total = $scope.WorkOrder.total.round(2);
        };

        $scope.WorkOrderList = [];
        $scope.ShowInvoiceList = false;

        $scope.SearchWorkOrder = function (id) {

            var request = {
                "Item": { id: id },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "MfgworkorderService/Search",
                data: request
            }).success(function (result) {
                $scope.ShowSearch = false;
                $scope.DisplayWorkOrder(result[0]);
            }).error(function (result) {
            });
        };


        $scope.Save = function (movenext) {

            if (typeof ($scope.WorkOrder.companyid) == 'undefined' || $scope.WorkOrder.companyid == null || $scope.WorkOrder.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select buying company name");
                return;
            }

            if ($scope.WorkOrder.wodate == null || $scope.WorkOrder.wodate == "") {
                $scope['setFocuswodate']();
                $rootScope.Toaster.Info("Please select Work Order Date");
                return;
            }

            var request = {
                "Item": $scope.WorkOrder,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;

            $http({
                method: 'POST',
                url: "MfgworkorderService/Save",
                data: srequest
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Work Order Saved Successfully.!");
                    $scope.WorkOrder.id = result.id;
                    $scope.WorkOrder.workorderno = result.workorderno;
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

        $scope.DisplayWorkOrder = function (item) {
            $scope.ItemDetails.Customize = false;
            $scope.Navigation.SetMode('EDIT');
            $scope.WorkOrder = item;
            $scope.ShowSearch = false;
            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
            $scope.CalculateTotal();
            $.each($scope.WorkOrder.Mfgworkorderitems, function (k, v) {
                v.sno = k + 1;
            });
            if ($scope.WorkOrder.Mfgworkorderitems.length > 0)
                $scope.ItemDetails.sno = $scope.WorkOrder.Mfgworkorderitems[$scope.WorkOrder.Mfgworkorderitems.length - 1].sno + 1;
            else
                $scope.ItemDetails.sno = 1;

            $scope.WorkOrder.sizelist.sort(function (a, b) {
                return a.sizeindex - b.sizeindex;
            });
            if ($scope.IsSampleBased) {
                $scope.CustomerChanged();
            }
            $timeout(function () {
                if ($scope.WorkOrder.onlybom && !$scope.IsSampleBased)
                    $scope.ConfigureBOMGrid(100);
                else
                    $scope.ConfigureGrid(100);
            }, 25);
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $scope.WorkOrder.Mfgworkorderitems.splice($scope.WorkOrder.Mfgworkorderitems.indexOf(item), 1);
                if ($scope.WorkOrder.Mfgworkorderitems.length == 0) {
                    $scope.ItemDetails.sno = 1;
                }

                $scope.CalculateTotal();
                $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');
            }
            else if (action == "Edit") {
                if (typeof ($scope.ItemDetails.IsEditItem) != 'undefined' &&
                    $scope.ItemDetails.IsEditItem) {
                    $rootScope.Confirm('Editing', 'you will loose the current editing item', function () {
                        $timeout(function () {
                            $scope.EditItem(item);
                            $scope.ItemDetails.IsEditItem = true;
                        }, 10);
                    });
                }
                else {
                    $timeout(function () {
                        $scope.EditItem(item);
                        $scope.ItemDetails.IsEditItem = true;
                    }, 10);
                }

            }
            else if (action == "EditWorkOrder") {
                $scope.SearchWorkOrder(item.id);

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
                                    url: "MfgworkorderService/Delete",
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
            $scope.WorkOrder.Mfgworkorderitems.splice($scope.WorkOrder.Mfgworkorderitems.indexOf(item), 1);
            $scope.CalculateTotal();
            $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');
            $scope.ItemDetails = item;
            $scope.ItemDetails.Customize = true;
            //if (item.id && item.id > 0) {
            //$scope.ItemDetails.designname = $scope.SearchRef($scope.ItemDetails.designid, $scope.DesignList);
            $scope.ItemDetails.Fabric = $scope.SearchRef($scope.ItemDetails.fabricid, $scope.FabricList);
            $scope.ItemDetails.Colour = $scope.SearchRef($scope.ItemDetails.colourid, $scope.AllColours);
            $scope.ItemDetails.Sleeve = $scope.SearchRef($scope.ItemDetails.sleeveid, $scope.SleeveList);
            //}
            var designid = $scope.ItemDetails.designid;
            var size = $scope.ItemDetails.size;
            var designitemname = $scope.ItemDetails.designitemname;
            $scope.ItemDetails.designid = null;
            $scope.ItemDetails.designitemname = null;
            $scope.fabricChange(function () {
                $timeout(function () {
                    $scope.ItemDetails.designid = designid;
                    $scope.designChanged(false, true, function () {
                        $timeout(function () {
                            $scope.ItemDetails.designitemname = designitemname;
                            $scope.ItemDetails.size = size;
                        }, 50);
                    });
                }, 50);
            });
            //$scope.ItemDetails.colourid = colourid;
        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgworkorderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ConfigSearchGrid();
                $scope.SearchResult = result;
                $scope.sgrid.UpdateData(result);
            }).error(function (result) {
            });
        }

        $scope.ConfigSearchGrid = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchgridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.BuyingCompany);
            igrid.AddSimpleColumn('workorderno', 'WO No', 80);
            igrid.AddSimpleColumn('jobcardno', 'Card No', 80);
            igrid.AddReferenceColumn('suppliercompanyid', 'Supplier', 100, $scope.SupplierList);
            igrid.AddFormatedColumn('wodate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.WarehouseLocations);
            igrid.AddReferenceColumn('issuedby', 'Issued', 100, $scope.Employee);
            igrid.AddSimpleColumn('product', 'Products', 100);
            igrid.AddSimpleColumn('design', 'Design', 100);
            igrid.AddSimpleColumn('totalqty', 'Qty', 60);
            igrid.AddSimpleColumn('total', 'Cost', 60);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Print'/>"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('EditWorkOrder', args.item);
                    else if ($(e.target).val() == 'Delete') {
                        $scope.ManageSearchItem('DeleteWorkOrder', args.item);
                    }
                    else if ($(e.target).val() == 'Print') {
                        $scope.Print(args.item.id);
                    }
                });
            });

            igrid.Initialize();
            $scope.sgrid = igrid;
        }

        $scope.ConfigSearchGrid();

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.WorkOrder', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.WorkOrder = value;
        };



        $scope.Print = function (workorderid) {
            $timeout(function () {

                var url = 'MfgworkorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (workorderid) != 'undefined' ? workorderid : $scope.WorkOrder.id);
                if ($scope.ReportList.length == 1) {
                    $rootScope.Download(url, 'application.pdf', 'application/pdf');
                }
                else {
                    var dialog = ngDialog.open({
                        template: 'ChooseTemplate',
                        data: {
                            'scope': $scope
                        },
                        width: '500px',
                        controller: ['$scope', '$timeout', function ($scope, $timeout) {
                            $scope.ReportList = $scope.ngDialogData.scope.ReportList;
                            $scope.Selected = function () {
                                $scope.closeThisDialog({
                                    'templateid': $scope.templateid
                                });
                            };
                        }],
                        className: 'ngdialog-theme-plain'
                    });
                    dialog.closePromise.then(function (data) {
                        {
                            if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                                url += '/'+data.value.templateid;
                                $rootScope.Download(url, 'application.pdf', 'application/pdf');
                            }
                        }
                    });
                }


                //var link = document.createElement('a');
                //link.download = "workorder.pdf";
                //link.href = 'MfgworkorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (workorderid) != 'undefined' ? workorderid : $scope.WorkOrder.id);
                //document.body.appendChild(link);
                //link.click();
                //$timeout(function () {
                //    link.remove();
                //}, 100);
            }, 10);
        }

        $scope.productChange = function () {
            $scope.DesignList = [];
            if ($scope.ItemDetails.productid && $scope.ItemDetails.productid) {
                restService.Get('MfgdesignService/SearchAll/' + $scope.ItemDetails.productid, true).then(function (results) {
                    $scope.DesignList = results;
                    $timeout(function () {
                        $scope.setFocusdeisgnid();
                    }, 10);
                });
            }
        }

        $scope.CustomerChanged = function () {
            restService.Get('ReferencelistService/MFGSAMPLES/' + $scope.WorkOrder.customerid, true).then(function (results) {
                $scope.CustomerSampleList = results;
            });
        }

        $scope.SampleQtyChanged = function () {
            var tqty = 0;
            $.each($scope.SampleSizeList, function (k, v) {
                var qty = parseFloat(v.qty);
                if (isNaN(qty)) qty = 0;
                tqty += qty;
            })
            $scope.ItemDetails.totalqtyqty = tqty;
        }

        $scope.SampleDesignChanged = function () {
            $scope.ItemDetails.imageid = null;
            if ($scope.ItemDetails.sampledesignid &&
                $scope.ItemDetails.sampledesignid > 0) {
                var sample = $scope.CustomerSampleDesignList.find(function (item) {
                    return item.id == $scope.ItemDetails.sampledesignid;
                });
                if (sample && sample != null) {
                    $scope.ItemDetails.colourid = sample.colourid;
                    $scope.ItemDetails.designname = sample.name;
                    $scope.ItemDetails.designitemname = sample.code;
                    var JumpList = [];
                    $.each(sample.sizes, function (k, v) {
                        JumpList.push({
                            'id': v.id,
                            'itemid':v.itemid,
                            'name': $scope.SearchRef(v.id, $scope.SizeList, '-'),
                            'uid': v.uid,
                            'cost': sample.requiredcost == 0 ? 0 : sample.requiredcost/1000,
                            'price': sample.requiredprice == 0 ? 0 : sample.requiredprice / 1000,
                            'time': sample.requiredhours == 0 ? 0 : sample.requiredhours / 1000
                        });
                    });
                    $scope.SampleSizeList = JumpList;
                }
            }
        }

        $scope.AddAllSOPending = function () {
            $.each($scope.SampleSizeList, function (k, v) {
                v.qty = v.oqty;
            });
            $scope.SampleQtyChanged();
        }

        $scope.SalesOrderChanged = function () {
            var so = $scope.CustomerSOList.find(function (item) {
                return $scope.ItemDetails.salesorderid == item.id;
            });

            $.each($scope.SampleSizeList, function (k, v) {
                v.oqty = '';
                v.qty = '';
            });

            if (so && so != null) {
                $.each(so.items, function (k, v) {
                    var soitem = $scope.SampleSizeList.find(function (item) {
                        return item.uid == v.uid;
                    });
                    if (soitem && soitem != null) {
                        soitem.oqty = v.qty;
                        soitem.itemid = v.itemid;
                        soitem.designid = v.designid;
                        soitem.sizegroup = v.sizegroup;
                        soitem.imageid = v.imageid;
                    }
                })
            }
        }

        $scope.SampleChanged = function () {
            $scope.ItemDetails.imageid = null;
            if ($scope.ItemDetails.sampleid &&
                $scope.ItemDetails.sampleid > 0) {
                var sample = $scope.CustomerSampleList.find(function (item) {
                    return item.id == $scope.ItemDetails.sampleid;
                });

                if (sample && sample != null) {
                    $scope.CustomerSampleDesignList = null;
                    restService.Get('ReferencelistService/MFGPENDINGSO/' + $scope.WorkOrder.customerid + '-' + $scope.ItemDetails.sampleid, true).then(function (results) {
                        $scope.CustomerSOList = results;
                        $scope.ItemDetails.sampleimageid = sample.imageid;
                        $scope.CustomerSampleDesignList = sample.designs;
                        $scope.ItemDetails.labelname = sample.name;
                        $scope.ItemDetails.productid = sample.prdid;
                        $scope.ItemDetails.mfgid = sample.mfgid;
                        $scope.ItemDetails.proddivid = sample.prddivid;
                        $scope.ItemDetails.prddivisionid = sample.prddivid;

                        var product = $scope.ProductList.find(function (item) {
                            return item.id == sample.prdid;
                        });

                        if (product && product != null) {
                            $scope.ItemDetails.taxid = product.ptaxid;
                            var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                                return (pitem.id == $scope.ItemDetails.taxid);
                            });
                            $scope.ItemDetails.taxpercentage = 0;
                            if (taxitem || taxitem != null) {
                                taxitem.taxsplit.findTax = $scope.findTax;
                                var taxconfig = taxitem.taxsplit.findTax(internalitem.cost);
                                if (taxconfig.cgst == 0) {
                                    taxconfig = { 'cgst': 0, 'sgst': 0 };
                                }
                                $scope.ItemDetails.taxpercentage = taxconfig.cgst + taxconfig.sgst;
                            }
                        }
                    });
                }
            }
        }

        $scope.AddSampleItem = function () {
            $scope.WorkOrder.mfgsampleorderid = $scope.ItemDetails.sampleid;
            $scope.WorkOrder.design = $scope.ItemDetails.labelname;
            $scope.WorkOrder.proddivid = $scope.ItemDetails.proddivid;
            $scope.WorkOrder.productid = $scope.ItemDetails.productid;
            $scope.WorkOrder.designid = $scope.ItemDetails.mfgid;
            $scope.WorkOrder.onlybom = false;
            $scope.WorkOrder.product = $scope.SearchRef($scope.ItemDetails.proddivid, $scope.ProductDivision);
            if ($scope.ItemDetails.id && $scope.ItemDetails.id > 0) {
                $scope.ItemDetails.Customize = false;
                $scope.ItemDetails.IsEditItem = false;
                $scope.WorkOrder.Mfgworkorderitems.push($scope.ItemDetails);
                $scope.igrid.UpdateData($scope.WorkOrder.Mfgworkorderitems, '_id');
                $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                $scope.ItemDetails.id = 0;
            }
            else {
                var internalitem = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                internalitem.taxamount = (internalitem.amount * internalitem.taxpercentage / 100).round(2);
                internalitem.amount += internalitem.taxamount;
                internalitem.isactive = true;
                internalitem.version = 1;
                internalitem.sizelist = [];
                $.each($scope.SampleSizeList, function (k, v) {
                    internalitem.sizelist.push({
                        'code': v.code,
                        'designid': v.designid,
                        'name': v.name,
                        'itemid':v.itemid,
                        'colourid': internalitem.colourid,
                        'sizeid': v.id,
                        'sizegroup': v.sizegroup,
                        'uid': v.uid,
                        'qty': v.qty,
                        'cost': v.cost,
                        'price': v.price,
                        'time': v.time,
                        'isactive': true,
                        'designid': $scope.ItemDetails.mfgid,
                        'version': 1
                    });
                })
                internalitem.sizecount = internalitem.sizelist.length;
                $scope.WorkOrder.Mfgworkorderitems.push(internalitem);
                $scope.ConfigureGrid(100);
                $scope.ItemDetails.Customize = false;
                $scope.CalculateTotal();
                $scope.ItemDetails.sno = parseInt($scope.WorkOrder.Mfgworkorderitems[$scope.WorkOrder.Mfgworkorderitems.length - 1].sno) + 1;
            }
            $scope.setFocussno();
            $scope.SampleSizeList = [];
            $scope.ItemDetails.sampleid = null;
            $scope.ItemDetails.sampledesignid = null;
            $scope.CustomerSampleDesignList = [];
        }

        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.mfg_mode = $rootScope.UserInfo.appsettings.mfg_mode;
                $scope.IsSampleBased = $scope.mfg_mode == 6;
                $scope.mfg_fabricwostock = $rootScope.UserInfo.appsettings.mfg_fabricwostock;
                $scope.mfg_fabricbasedorder = $rootScope.UserInfo.appsettings.mfg_fabricbasedorder;
                if (!$scope.mfg_fabricbasedorder) {
                    $scope.mfg_designclass = "col-md-5";
                }

                if ($scope.IsSampleBased) {
                    restService.Get('ReferencelistService/CUSTOMERLEDGERUNAPPROVED', true).then(function (results) {
                        $scope.Customers = results;
                    });

                    restService.Get('ReferencelistService/SIZE', true).then(function (results) {
                        $scope.SizeList = results;
                    });

                    restService.Get('ReferencelistService/FULL-PRODUCTDIVISION', true).then(function (results) {
                        $scope.ProductDivision = results;
                    });
                }

                if (typeof ($location.search().id) != 'undefined' &&
                    $location.search().id.length > 0) {
                    $timeout(function () {
                        $scope.SearchWorkOrder($location.search().id);
                    }, 100);
                }

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
