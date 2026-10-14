
var app = angular.module('InfinityLab');

app.controller('BOMBundlesController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', 'cntxService', '$timeout', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, cntxService, $timeout, Excel) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchConditionItems = [{ 'id': 1, 'name': 'Brand' }, { 'id': 2, 'name': 'Product' }, { 'id': 3, 'name': 'Barcoded On' }, { 'id': 4, 'name': 'Pending' }
            , { 'id': 6, 'name': 'LREntry' }, { 'id': 8, 'name': 'From File' }];

        $scope.intercompanytransfer = false;

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.ShowSearch = false;
        $scope.SearchResults = [];
        $scope.SearchBOMOutwardResult = [];
        $scope.SearchRef = restService.SearchReference;

        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });

        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandList = results;
        });
        $scope.DepartmentList = restService.Get('ReferencelistService/MFRDEPARTMENT', true).then(function (results) {
            $scope.DepartmentList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.Location = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.WHLocation = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WHLocation = results;
        });

        $scope.AllLocation = [];
        $scope.RetailLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.RetailLocation = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo == null || !v.additionalinfo.iswarehouse) {
                    $scope.RetailLocation.push(v);
                }
                $scope.AllLocation.push(v);
            });
        });

        $scope.BOMCategories = [];
        restService.Get('ReferencelistService/BOMCATEGORY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.BOMCategories.push(v);
            });
        });

        $scope.BOMAllList = [];
        restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
        });

        $scope.ColorList = [];
        $scope.AllColorList = [];
        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
            $.each(results, function (k, v) {
                $scope.AllColorList.push(v);
            });
        });

        $scope.BOMTypeList = restService.Get('ReferencelistService/BOMTYPE', true).then(function (results) {
            $scope.BOMTypeList = results;
        });
        $scope.BOMDept = restService.Get('ReferencelistService/BOMDEPARTMENT', true).then(function (results) {
            $scope.BOMDept = results;
        });

        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });

        $scope.LocalContext = cntxService.CreatePageContext('outward');
        $scope.TotalAccepted = 0;
        $scope.SearchCounts = 0;
        $scope.Entity = {
            "id": 0,
            "items_json": null,
            "selectAll": false,
            "code": null,
            "companyid": 0,
            "fromlocationid": 0,
            "tolocationid": 0,
            "fromdepartmentid": 0,
            "todepartmentid": 0,
            "packedby": 0,
            "receivedby": 0,
            "packedon": null,
            "notes": null,
            "approvedby": 0,
            "isactive": true,
            "createdon": "",
            "modifiedon": "",
            "createdby": 0,
            "modifiedby": 0,
            "status": 0,
            "qty": 0,
            "version": 1,
            "cansave": true,
            "candelete": false,
            "items": {},
            "checkList": true,
            tocompanyid: ""
        };
        $scope.BOMOutwards = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.BOMOutwards.items = [];

        $scope.SaveOutList = function () {
            if (typeof ($scope.BOMOutwards.items) == 'undefined' || $scope.BOMOutwards.items == null)
                $scope.BOMOutwards.items = [];

            $scope.BOMOutwards.items.push(
                {
                    'bomcategoryid': $scope.BOMOutwardsItem.bomcategoryid,
                    'bomid': $scope.BOMOutwardsItem.bomid,
                    'bomcode': $scope.BOMOutwardsItem.bomcode,
                    'qty': $scope.BOMOutwardsItem.qty,
                    'width': $scope.BOMOutwardsItem.width,
                    'cost': $scope.BOMOutwardsItem.cost,
                    'colourid': $scope.BOMOutwardsItem.colourid,
                    'imageid': $scope.BOMOutwardsItem.imageid,
                    'sno': $scope.BOMOutwards.items.length + 1
                });

            if ($scope.BOMOutwards.items.length == 1)
                $scope.OutConfig();
            else
                $scope.igrid.UpdateData($scope.BOMOutwards.items, '_id');
            $scope.setFocusbomcode();
        }

        $scope.categoryChange = function (callback) {
            if (typeof ($scope.BOMOutwardsItem.bomcategoryid) != 'undefined' && $scope.BOMOutwardsItem.bomcategoryid != null) {
                var BOMList = [];
                $.each($scope.BOMAllList, function (k, v) {
                    if (v.categoryid == $scope.BOMOutwardsItem.bomcategoryid)
                        BOMList.push(v);
                });
                $scope.BOMList = BOMList;
            }
            if (callback)
                $timeout(callback, 20);
        }

        $scope.productChange = function (callback) {
            if ($scope.BOMOutwardsItem.bomid != null) {
                var prditem = $scope.BOMList.find(function (item) {
                    return (item.id == $scope.BOMOutwardsItem.bomid);
                });
                if (prditem || prditem != null) {
                    $scope.SetColourList(prditem);
                    $scope.BOMOutwardsItem.cost = prditem.buyingrate;
                }
            };
            if (callback) $timeout(callback, 20);
        };

        $scope.SetColourList = function (prditem) {
            $scope.BOMOutwardsItem.colourid = null;
            $scope.BOMOutwardsItem.bominwardcode = '';
            if (prditem.colourmode == 2) {
                $scope.ColorList = [];
                $.each(prditem.colourlist, function (k, v) {
                    var colour = $scope.AllColorList.find(function (item) {
                        return item.id == v.id;
                    });
                    if (colour && colour.id > 0) {
                        colour.bominwardcode = v.name;
                        colour.imageid = v.imageid;
                        $scope.ColorList.push(colour);
                    }
                });
            } else if (prditem.colourmode == 1) {
                $scope.ColorList = [];
                var colour = $scope.AllColorList.find(function (item) {
                    return item.id == prditem.colorid;
                });
                if (colour && colour.id > 0) {
                    colour.bominwardcode = v.name;
                    colour.imageid = v.imageid;
                    $scope.ColorList.push(colour);
                }
            }
            else {
                $scope.ColorList = [];
                $scope.BOMOutwardsItem.bominwardcode = prditem.code;
                $scope.BOMOutwardsItem.imageid = prditem.imageid;
            }
        }

        $scope.setColourFocus = function () {
            if ($scope.ColorList.length > 0)
                $scope.setFocuscolourid();
            else
                $scope.setFocusqty();
        }

        $scope.OutConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '100px', '100%', true, false, true, 50);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 50);
            igrid.AddReferenceColumn('bomcategoryid', 'Category', 100, $scope.BOMCategories);
            igrid.AddReferenceColumn('bomid', 'Code', 150, $scope.BOMAllList);

            column = igrid.AddCustomColumn('bomname', 'Name', 120, function (item, col) {
                var bomitem = col.datasource.find(function (sitem) {
                    return sitem.id == item.bomid;
                });
                return (bomitem && bomitem != null) ? bomitem.title : '';
            });
            column.datasource = $scope.BOMAllList;
            column.enablefilter = true;
            column.getValue = function (item, col) {
                var bomitem = col.datasource.find(function (sitem) {
                    return sitem.id == item.bomid;
                });
                return (bomitem && bomitem != null) ? bomitem.title : '';
            }

            igrid.AddReferenceColumn('colourid', 'Colour', 80, $scope.AllColorList);
            igrid.AddSimpleColumn('bomcode', 'Barcode', 80);
            igrid.AddSimpleColumn('width', 'Width', 80);
            igrid.AddFormatedColumn('cost', 'Cost', 80, 'N4', 4);
            if ($scope.BOMRequest) {
                igrid.AddFormatedColumn('requestqty', 'Requested', 80, 'N4', 4).total = true;
            }

            var column = igrid.AddColumn(
                {
                    id: "qty", name: "Qty", field: "qty",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, format: 'N4', decimal: 4
                }
            );
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                var v = parseFloat(item.qty);
                if (isNaN(v)) v = 0;
                return v;
            };
            column.InfyGrid = igrid;
            column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                var v = parseFloat(dataContext.qty);
                if (isNaN(v)) v = 0;
                return columnDef.InfyGrid.FormatNumber(v, 4);
            }

            if ($scope.ShowStock) {
                igrid.AddFormatedColumn('stockqty', 'Stock', 80, 'N4', 4).total = true;
                igrid.AddFormatedColumn('poqty', 'P.O', 80, 'N4', 4).total = true;
            }

            if ($scope.BOMOutwards.id == 0 || $scope.BOMOutwards.status == 0) {
                var commands = "";
                var commands = "";
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

                if ($rootScope.UserInfo.p_PRODUCTMASTER.modify || $rootScope.UserInfo.p_PRODUCTMASTER.delete) {
                    igrid.AddButtonColumn('action', 'ACTION', 150, commands, function (e, args) {
                        $scope.$apply(function () {
                            if ($(e.target).val() == 'Delete')
                                $scope.ManageList(args.item);
                        });
                    });
                }
            }
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'qty') {
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.igrid = igrid;
            //$.each($scope.BOMOutwards.items, function (k, v) {
            //    var bom = $scope.BOMAllList.find(function (item) {
            //        return item.id == v.bomid;
            //    });
            //    if (bom && bom != null)
            //        v.bomname = bom.name;
            //})
            $scope.igrid.UpdateData($scope.BOMOutwards.items, '_id');
        }

        $scope.ShowStock = false;
        $scope.LoadBOMStock = function () {

            var request = {
                "Item": {
                    'mode':3,
                    'companyid': $scope.BOMOutwards.companyid,
                    'fromlocationid': $scope.BOMOutwards.fromlocationid,
                    'bomids':[]
                },
                "securitytoken": "String content"
            }
            $.each($scope.BOMOutwards.items, function (k, v) {
                request.Item.bomids.push(v.bomid);
            });
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: 'BomstocktransactionService/StockReport',
                data: srequest
            }).success(function (result) {

                $.each($scope.BOMOutwards.items, function (k, v) {
                    var stockitem = result.find(function (item) {
                        return item.bomid == v.bomid && item.colourid == v.colourid;
                    });
                    if (stockitem && stockitem != null) {
                        v.stockqty = stockitem.closing;
                        v.poqty = stockitem.poqty;
                    }
                });
                $scope.ShowStock = true;
                $scope.OutConfig();
                $scope.ShowStock = false;
            });
        }

        $scope.ManageList = function (item) {
            $scope.BOMOutwards.items.splice($scope.BOMOutwards.items.indexOf(item), 1);
            $scope.igrid.UpdateData($scope.BOMOutwards.items, '_id');
        };

        $scope.PrintList = [];
        $scope.ShowPrint = true;
        $scope.totalqty = 0;

        $scope.PushItems = function () {
            var removeitem = [];
            $.each($scope.SearchResults, function (key, value) {
                if (value.selected) {

                    var existingitem = $scope.BOMOutwards.items.find(function (item) {
                        return item.stockid == value.stockid;
                    })

                    if (typeof (existingitem) == 'undefined' || existingitem == null) {
                        value.iqty = value.qty;
                        $scope.BOMOutwards.items.push(value);
                    }
                    removeitem.push(value);
                }
            });


            $scope.BOMOutwards.packedon = new Date();

            $scope.BOMOutwards.qty = 0;
            $.each($scope.BOMOutwards.items, function (k, v) {
                $scope.BOMOutwards.qty += v.qty;
            });


            $.each(removeitem, function (k, v) {
                var index = $scope.SearchResults.indexOf(v);
                $scope.SearchResults.splice(index, 1);
                v.selected = false;
            });

            $scope.SearchCounts = 0;
            $.each($scope.SearchResults, function (k, v) {
                $scope.SearchCounts += (v.ismeter ? 1 : v.qty);;
            });

            $scope.LocalContext.Post('$scope.BOMOutwards.items', $scope.BOMOutwards.items, false);
        };

        $scope.fileuploadedcomplete = function (value) {
            var barcodelist = [];
            $.each(value.rows, function (k, v) {
                barcodelist.push(v.values[0]);
            });
            $scope.SearchStock(true, barcodelist.join(','));
        };


        $scope.PopItems = function () {
            var removeitem = [];
            $.each($scope.BOMOutwards.items, function (key, value) {
                if (value.selected) {
                    $scope.SearchResults.push(value);
                    removeitem.push(value);
                }
            });

            $.each(removeitem, function (k, v) {
                $scope.BOMOutwards.items.splice($scope.BOMOutwards.items.indexOf(v), 1);
                v.selected = false;
            });

            $scope.BOMOutwards.qty = 0;
            $.each($scope.BOMOutwards.items, function (k, v) {
                $scope.BOMOutwards.qty += v.qty;
            });

            $scope.SearchCounts = 0;
            $.each($scope.SearchResults, function (k, v) {
                $scope.SearchCounts += (v.ismeter ? 1 : v.qty);;
            });
        };

        $scope.SearchOutwords = {
            'fromwarehouseBOMOutward': true, 'todate': null, 'fromdate': null
        };

        $scope.SearchCondition = {
            id: 0,
            date: null,
            addedfrom: null,
            addedto: null,
            productid: null,
            brandid: null,
            barcode: null,
            companyid: null,
            searchnewbarcodes: false,
            searchtype: 4,
            supplierid: [],
            suppliername: '',
            lrentryno: null,
            selected: false
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.BOMOutwards.items', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.BOMOutwards.items = value;
            $scope.BOMOutwards.qty = 0;
            $.each($scope.BOMOutwards.items, function (k, v) {
                $scope.BOMOutwards.qty += v.qty;
            });

        };

        $scope.currentcompanyid = 0;
        $scope.SearchStock = function (barcoded, barcodelist) {

            if ($scope.Appsetting_Company &&
                (typeof ($scope.BOMOutwards.companyid) == 'undefined' || $scope.BOMOutwards.companyid <= 0)) {
                $rootScope.Toaster.Info(" Please select company");
                return;
            }

            if ($scope.currentcompanyid == 0) {
                $scope.currentcompanyid = $scope.BOMOutwards.companyid;
            }
            else if ($scope.currentcompanyid != $scope.BOMOutwards.companyid) {
                $scope.currentcompanyid = $scope.BOMOutwards.companyid;
                $scope.SearchResults = [];
                $scope.BOMOutwards.items = [];
            }


            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            if (barcoded) {
                request = {
                    "Item": { 'barcode': barcodelist, 'companyid': $scope.BOMOutwards.companyid, 'searchtype': 5 },
                    "securitytoken": "String content"
                }
            }
            else {
                $scope.SearchCondition.companyid = $scope.BOMOutwards.companyid;
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/SearchStock",
                data: srequest
            }).success(function (result) {

                if (angular.isArray(result)) {

                    $scope.SearchResults = [];

                    $.each(result, function (k, v) {

                        if (barcoded) {
                            var itm = $scope.BOMOutwards.items.find(function (itm) {
                                return v.stockid == itm.stockid;
                            });
                            if (typeof (itm) == 'undefined' || itm == null) {
                                //$scope.BOMOutwards.items.push(v);
                                $scope.BOMOutwards.items.splice(0, 0, v);
                                $scope.SearchCondition.directbarcode = "";
                                if (!v.ismeter && $scope.BOMOutwards.multiscan) {
                                    v.iqty = 1;
                                }

                                $scope.LocalContext.Post('$scope.BOMOutwards.items', $scope.BOMOutwards.items, false);
                            }
                            else {
                                if (!v.ismeter && $scope.BOMOutwards.multiscan) {
                                    itm.iqty += 1;
                                }
                                else {
                                    itm.qty = v.qty;
                                    itm.iqty = v.iqty;
                                }
                                $scope.SearchCondition.directbarcode = "";
                                $scope.LocalContext.Post('$scope.BOMOutwards.items', $scope.BOMOutwards.items, false);
                            }
                        }
                        else {
                            var itm = $scope.SearchResults.find(function (itm) {
                                return v.stockid == itm.stockid;
                            });
                            if (typeof (itm) == 'undefined' || itm == null)
                                $scope.SearchResults.push(v);

                            if ($scope.SearchCondition.searchtype == 5) {
                                $scope.SearchCondition.barcode = "";
                            }
                        }

                    });
                }


                $scope.CalculateTotal();

            }).error(function (result) {
            });
        }

        $scope.CalculateTotal = function () {
            $scope.BOMOutwards.qty = 0;
            $.each($scope.BOMOutwards.items, function (k, v) {
                $scope.BOMOutwards.qty += v.qty;
            });
        }

        $scope.selectAllItems = function (items, mode) {

            $.each(items, function (key, value) {
                value.selected = mode;
            });

            /*if ($scope.BOMOutwards.selectAll) {
                $.each($scope.SearchResults, function (key, value) {
                    $scope.SearchResults[key].selected = true;
                });
            }
                else {
                    $.each($scope.SearchResults, function (key, value) {
                        $scope.SearchResults[key].selected = false;
                    });
            }*/

        }

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.BOMOutwards = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.BOMOutwards = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResults = [];
            $scope.BOMOutwards.items = [];
            $scope.TotalAccepted = 0;
            $scope.OutConfig();
        };

        $scope.Save = function () {
            var iserror = false;
            var wrongbarcode = '';
            $scope.BOMOutwards.qty = 0;


            var requiredstock = true;
            $.each($scope.BOMOutwards.items, function (k, v) {
                v.qty = parseFloat(v.qty);
                if (isNaN(v.qty)) v.qty = 0;
                var stk = (v.stockqty == undefined ? 0 : v.stockqty) + (v.poqty == undefined ? 0 : v.poqty);
                if (v.qty < v.iqty || v.iqty <= 0) {
                    wrongbarcode = v.barcode;
                    iserror = true;
                }
                if (v.qty < stk) {
                    requiredstock = false;
                }
                $scope.BOMOutwards.qty += v.qty;
            });

            if (!requiredstock) {
                $rootScope.ShowAlert('Stock Required', 'Required stock not avilable');
                return;
            }

            if (iserror) {
                $rootScope.ShowAlert('No data found', 'Wrong Out qty for - ' + wrongbarcode);
                return;
            }

            if ($scope.BOMOutwards.packedon == null) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info(" Please select date");
                return;
            }
            if ($scope.BOMOutwards.fromcompanyid == null || $scope.BOMOutwards.fromcompanyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select company name");
                return;
            }

            if ($scope.BOMOutwards.packedby == null || $scope.BOMOutwards.packedby == 0) {
                $scope['setFocuspackedby']();
                $rootScope.Toaster.Info("Please select packed by");
                return;
            }
            if ($scope.BOMOutwards.fromlocationid == null || $scope.BOMOutwards.fromlocationid == 0) {
                $scope['setFocusfromlocationid']();
                $rootScope.Toaster.Info("Please select from location");
                return;
            }

            if ($scope.BOMOutwards.tolocationid == null || $scope.BOMOutwards.tolocationid == 0) {
                $scope['setFocustolocationid']();
                $rootScope.Toaster.Info("Please select to location");
                return;
            }

            if ($scope.BOMOutwards.fromlocationid == $scope.BOMOutwards.tolocationid) {
                $scope['setFocustolocationid']();
                $rootScope.Toaster.Info("Please select from/to different location");
                return;
            }

            $scope.BOMOutwards.inward2warehouse = true;

            request = {
                "Item": $scope.BOMOutwards,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.LocalContext.Post('$scope.BOMOutwards.items', null, false);
                    $rootScope.Toaster.Info("BOMOutwards Saved Successfully.!");
                    $scope.BOMOutwards.code = result.code;
                }
                else {
                    $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {
            });
        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchOutwords,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchBOMOutwardResult = result;

                $.each(result, function (k, v) {

                    v.cansave = false;
                    v.candelete = $scope.BOMOutwards.status == 0
                    v.statusdisplay = '';
                    v.canreject = false;
                    switch (v.status) {
                        case 0:
                            v.statusdisplay = 'New Bundle';
                            break;
                        case 1:
                            v.statusdisplay = 'Inwarded Bundle';
                            break;
                        case 2:
                            v.statusdisplay = 'Inwarded Bundle with changes';
                            break;
                        case 9:
                            v.statusdisplay = 'Rejected Bundle';
                            break;
                    }

                })

                $scope.OutConfig();

            }).error(function (result) {
            });
        }


        $scope.SearchByCode = function (id) {

            if (typeof ($scope.BOMOutwards.code) == 'undefined' ||
                $scope.BOMOutwards.code == null ||
                $scope.BOMOutwards.code == '') {
                if (typeof (id) == 'undefined')
                    return;
            }

            request = {
                "Item": {
                    'code': $scope.BOMOutwards.code,
                    'id': id,
                    'fromwarehouseoutward': true
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Search",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.LoadBOMOutward(result[0]);
                }
                else {
                    $scope.BOMOutwards.statusdisplay = 'Not Found!';
                }
            }).error(function (result) {
            });
        }

        $scope.LoadBOMOutward = function (item) {
            $scope.BOMOutwards = item;
            $scope.BOMOutwards.cansave = $scope.BOMOutwards.status == 0;
            $scope.BOMOutwards.candelete = $scope.BOMOutwards.status == 0
            $scope.BOMOutwards.statusdisplay = '';
            $scope.BOMOutwards.canreject = false;
            switch ($scope.BOMOutwards.status) {
                case 0:
                    $scope.BOMOutwards.statusdisplay = 'New Bundle';
                    break;
                case 1:
                    $scope.BOMOutwards.statusdisplay = 'Inwarded Bundle';
                    break;
                case 2:
                    $scope.BOMOutwards.statusdisplay = 'Inwarded Bundle with changes';
                    break;
                case 9:
                    $scope.BOMOutwards.statusdisplay = 'Rejected Bundle';
                    break;
            }

            if ($scope.BOMOutwards.status == 9)
                $scope.BOMOutwards.id = 0;

            $scope.TotalCounts = 0;
            $scope.TotalAccepted = 0;
            $.each($scope.BOMOutwards.items, function (k, v) {
                v.sno = k + 1;
                if ($scope.BOMRequest) {
                    v.qty = v.qty.round(4);
                }
                $scope.TotalCounts += v.iqty;
                if (v.aqty > 0)
                    $scope.TotalAccepted += v.aqty;
                if ($scope.BOMRequest) {
                    v.requestqty = v.qty;
                }
            });
            $scope.Navigation.SetMode('EDIT');
            $scope.ShowSearch = false;
            $timeout(function () {
                $scope.OutConfig();
            }, 50);
        }

        $scope.ShowSuppliers = function () {
            var dialog = ngDialog.open({
                template: 'SupplierSearch',
                data: [$scope, $scope.SupplierList],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SupplierList = $scope.ngDialogData[1];
                    $scope.selectall = true;
                    $.each($scope.SupplierList, function (k, v) {
                        v.selected = true;
                    });

                    $scope.selectallChanged = function () {
                        $.each($scope.SupplierList, function (k, v) {
                            v.selected = $scope.selectall;
                        });
                    };

                    $scope.Selected = function () {
                        var ret = {
                            'name': '', supplierid: []
                        };
                        $.each($scope.SupplierList, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected) {
                                ret.name += (ret.name.length > 0 ? ',' : '') + v.name;
                                ret.supplierid.push(v.id);
                            }
                        });
                        $scope.closeThisDialog(ret);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.SearchCondition.suppliername = data.value.name;
                        $scope.SearchCondition.supplierid = data.value.supplierid;
                    }
                }
            });
        }

        $scope.ManageSearchItem = function (action, item) {

            if (action == 'Delete') {
                request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BOMBundlesService/Delete",
                    data: srequest
                }).success(function (result) {

                    if (result) {
                        $rootScope.Toaster.Info("Successfully Removed");
                    }
                    else {
                        $rootScope.Toaster.Info("Some of the items already sold, can't reject");
                    }

                }).error(function (result) {
                });
            } else if (action == "Edit") {
                $scope.SearchByCode(item.id);
            }
        }


        $scope.Delete = function () {
            request = {
                "Item": $scope.BOMOutwards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMBundlesService/Delete",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.statusdisplay = "Successfully Removed";
                    $scope.ErrorStyle = "success";
                    $scope.BOMOutwards = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.TotalCounts = 0;
                    $scope.SearchCounts = 0;
                    $scope.SearchResults = [];
                }
                else {
                    $scope.statusdisplay = "Some of the items already sold, can't reject";
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }
        $scope.Reject = function () {
            request = {
                "Item": $scope.BOMOutwards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMBundlesService/Reject",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.ErrorMessage = "Successfully Removed";
                    $scope.ErrorStyle = "success";
                }
                else {
                    $scope.ErrorMessage = "Some of the items already sold, can't reject";
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }

        $scope.Clear();

        var pagesize = 20;

        $scope.PrintPage = function () {
            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = "BOMOutwardS SLIP";

                var docHead = document.head.outerHTML;

                var table = $('#printdiv');
                var ctable = table.clone();

                var width = 800, height = 900;
                var printContents = ctable[0].outerHTML;
                var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=" + width + ", height=" + height + ", resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                var newWin = window.open("", "_blank", winAttr);
                var writeDoc = newWin.document;
                writeDoc.open();
                writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                writeDoc.close();
                newWin.focus();

                angular.element('#apptitle').context.title = otitle;
            }, 100);
        }

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'BOMOutward');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "BOMOutward.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.PrintPDF = function (item) {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "debitnote.pdf";
                link.href = 'BOMBundlesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.BOMOutwards.id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }
        $scope.PrintDocumentList = [];
        $scope.PrintOW = function () {
            $scope.totalqty = 0.0;

            request = {
                "Item": { 'id': $scope.BOMOutwards.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Print",
                data: srequest
            }).success(function (result) {
                $scope.PrintDocument = result;
                $scope.PrintDocumentList = [];
                var items = [];

                $.each(result.items, function (k, v) {
                    var exist = items.find(function (item) {
                        return item.productid == v.productid;
                    });
                    if (exist == null) {
                        var item = (JSON.parse(JSON.stringify(v)));
                        items.push(item);
                    }
                    else {
                        exist.iqty += v.iqty;
                    }
                });

                $.each(items, function (k, v) {
                    $scope.PrintDocumentList.push(v);
                });

                $.each($scope.PrintDocumentList, function (k, v) {
                    $scope.totalqty += v.iqty;
                });
                $scope.PrintPage();

            }).error(function (result) {
            });
        }

        $scope.searchbominwardcode = function (updatestock) {
            if (updatestock) {

                if ($scope.BOMOutwards.fromlocationid == undefined ||
                    $scope.BOMOutwards.fromlocationid == null ||
                    $scope.BOMOutwards.fromlocationid == '' ||
                    $scope.BOMOutwards.fromlocationid <= 0) {
                    $rootScope.Toaster.Error("Please select location!");
                    return;
                }

                if ($scope.stockbomcode &&
                    $scope.stockbomcode != null &&
                    $scope.stockbomcode.length > 0) {
                    restService.Get('BominvoiceitemsService/SearchBOMStock/' + $scope.stockbomcode, false).then(function (results) {
                        if (results != null && results.bominvoiceitem != null &&
                            results.stocksummary != null) {
                            var locationstock = results.stocksummary.find(function (item) {
                                return item.locationid == $scope.BOMOutwards.locationid;
                            });
                            if (locationstock && locationstock != null &&
                                locationstock.closing > 0) {
                                var consum = $scope.BOMOutwards.items.find(function (item) {
                                    return item.bomid == results.bominvoiceitem.bomid &&
                                        item.colourid == results.bominvoiceitem.colourid;
                                });
                                if (consum.qty < locationstock.closing) {
                                    consum.bomcode = $scope.stockbomcode;
                                    consum.bominvoiceitemid = results.bominvoiceitem.id;
                                }
                                else {
                                    var consum1 = (JSON.parse(JSON.stringify(consum)));
                                    consum1.qty = consum.qty - locationstock.closing;
                                    consum1.amount = consum1.qty * consum1.rate;
                                    $scope.BOMOutwards.items.push(consum1);
                                    consum.qty = locationstock.closing;
                                    consum.amount = consum.qty * consum.rate;
                                }
                                $scope.igrid.UpdateData($scope.BOMOutwards.items, '_id');
                                $scope.stockbomcode = '';
                            }
                        }
                    });

                }
            }
            else if ($scope.BOMOutwards.Details.bomcode &&
                $scope.BOMOutwards.Details.bomcode != null &&
                $scope.BOMOutwards.Details.bomcode.length > 0) {

                $scope.FindBomStock();

                restService.Get('BominvoiceitemsService/SearchCode/' + $scope.BOMOutwards.Details.bomcode, false).then(function (results) {
                    if (results != null) {
                        $scope.BOMOutwards.Details.bomcategoryid = results.bomcategoryid;
                        $scope.BOMOutwards.Details.bominvoiceitemid = results.id;
                        $scope.BOMOutwards.Details.bomid = results.bomid;
                        $scope.BOMOutwards.Details.colourid = results.colourid;
                        $scope.BOMOutwards.Details.width = results.width;
                        $scope.BOMOutwards.Details.rate = results.buyingprice;
                        $scope.setFocuswidth();
                    }
                });
            }
        }

        $scope.Appsetting_Company = false;
        //$scope.LoadApplicationSetting = function () {
        //    if (typeof ($rootScope.UserInfo) != 'undefined' &&
        //        typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
        //        $scope.Appsetting_Company = true;
        //    }
        //    else {
        //        $timeout(function () {
        //            $scope.LoadApplicationSetting();
        //        }, 50);
        //    }
        //}

        $scope.BOMOutwardsItem = {};
        $scope.searchbominwardcode = function (from) {
            if ($scope.BOMOutwardsItem.bomcode &&
                $scope.BOMOutwardsItem.bomcode != null &&
                $scope.BOMOutwardsItem.bomcode.length > 0) {

                restService.Get('BominvoiceitemsService/SearchCode/' + $scope.BOMOutwardsItem.bomcode, false).then(function (results) {
                    if (results == null) {
                        $scope.setFocuscategoryid();
                    }
                    else {
                        $scope.BOMOutwardsItem.bomcategoryid = results.bomcategoryid;
                        $scope.BOMOutwardsItem.bominvoiceitemid = results.id;
                        $scope.categoryChange(function () {
                            $scope.BOMOutwardsItem.bomid = results.bomid;
                            $scope.productChange(function () {
                                $scope.BOMOutwardsItem.colourid = results.colourid;
                                $scope.BOMOutwardsItem.width = results.width;
                                $scope.BOMOutwardsItem.cost = results.buyingprice;
                            });
                        });
                        $scope.setFocusqty();
                    }
                });

            }
            else {
                $scope.setFocuscategoryid();
            }
        }

        $scope.BOMRequest = false;
        $timeout(function () {
            if (typeof ($location.search().requestid) != 'undefined' &&
                $location.search().requestid.length > 0) {
                $scope.BOMRequest = true;
                request = {
                    "Item": {
                        'bomrequestid': $location.search().requestid,
                    },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BOMBundlesService/Search",
                    data: srequest
                }).success(function (result) {
                    if ($.isArray(result) && result.length > 0) {
                        $scope.LoadBOMOutward(result[0]);
                    }
                    else {
                        $scope.BOMOutwards.statusdisplay = 'Not Found!';
                    }
                }).error(function (result) {
                });
            }
            $scope.OutConfig();

        }, 10);

        $scope.CreateNew = function () {
            $scope.BOMOutwards.packedon = new Date();
        }();
    }]);
