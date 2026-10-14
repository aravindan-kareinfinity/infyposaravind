
var app = angular.module('InfinityLab');

app.controller('OutwardsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', 'cntxService', '$timeout', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, cntxService, $timeout, Excel) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Permission = UserDetails.Info;
        $scope.SearchConditionItems = [{ 'id': 1, 'name': 'Brand' }, { 'id': 2, 'name': 'Product' }, { 'id': 9, 'name': 'DesignNo' },
        { 'id': 3, 'name': 'Barcoded On' }, { 'id': 4, 'name': 'Pending' }, { 'id': 6, 'name': 'LREntry' }, { 'id': 8, 'name': 'From File' }];

        $scope.StatusList = [{ 'id': -1, 'name': 'All Transfers' },
        { 'id': 0, 'name': 'New Bundle' }, { 'id': 1, 'name': 'Inwarded Bundle' },
        { 'id': 2, 'name': 'Inwarded Bundle with changes' }, { 'id': 9, 'name': 'Rejected Bundle' }];

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.City = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.City = results;
        });
        $scope.ShowSearch = false;
        $scope.SearchResults = [];
        $scope.SearchOutwardResult = [];
        $scope.SearchRef = restService.SearchReference;

        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });

        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.Location = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Location = results;
            $scope.AllLocation = results;
        });

        $scope.WHLocation = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WHLocation = results;
            $scope.AllWHLocation = results;
        });

        $scope.RetailLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.RetailLocation = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo == null || !v.additionalinfo.iswarehouse) {
                    $scope.RetailLocation.push(v);
                }
            });
            $scope.AllRetailLocation = $scope.RetailLocation;
            //$rootScope.BindLocation($scope, 'OutWards.tolocationid');
        });

        $scope.FilterLocation = function (companyid, type) {
            var filterArray = null;
            var sourcearray = null;
            if (type == 'RetailLocation') {
                filterArray = $scope.RetailLocation = [];
                sourcearray = $scope.AllRetailLocation;
            }
            else if (type == 'Location') {
                filterArray = $scope.Location = [];
                sourcearray = $scope.AllLocation;
            }
            else if (type == 'WHLocation') {
                filterArray = $scope.WHLocation = [];
                sourcearray = $scope.AllWHLocation;
            }

            $.each(sourcearray, function (k, v) {
                if (v.additionalinfo.parentid <= 0 || v.additionalinfo.parentid == companyid) {
                    filterArray.push(v);
                }
            });
        }

        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });

        $scope.LocalContext = cntxService.CreatePageContext('outward');
        $scope.TotalAccepted = 0;
        $scope.TotalCounts = 0;
        $scope.SearchCounts = 0;
        $scope.Entity = {
            "id": 0,
            "items_json": null,
            "Items": [],
            "selectAll": false,
            "code": null,
            "companyid": 0,
            "fromlocationid": 0,
            "tolocationid": 0,
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
            "version": 1,
            "cansave": true,
            "candelete": false,
            "salablegoods": [],
            "checkList": true,
            tocompanyid: ""
        };
        $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.PrintList = [];
        $scope.ShowPrint = true;
        $scope.totalqty = 0;

        $scope.PushItems = function () {
            var removeitem = [];
            $.each(this.sgrid.GetSelectedRows(), function (key, value) {

                var existingitem = $scope.OutWards.items.find(function (item) {
                    return item.stockid == value.stockid;
                })

                if (typeof (existingitem) == 'undefined' || existingitem == null) {
                    value.iqty = value.qty;
                    $scope.OutWards.items.push(value);
                }
                removeitem.push(value);

            });


            $scope.OutWards.packedon = new Date();


            $scope.TotalCounts = 0;
            $.each($scope.OutWards.items, function (k, v) {
                $scope.TotalCounts += (v.ismeter ? 1 : $rootScope.GetNumber(v.iqty));
            });


            $.each(removeitem, function (k, v) {
                var index = $scope.SearchResults.indexOf(v);
                $scope.SearchResults.splice(index, 1);
                v.selected = false;
            });

            $scope.SearchCounts = 0;
            $.each($scope.SearchResults, function (k, v) {
                $scope.SearchCounts += (v.ismeter ? 1 : $rootScope.GetNumber(v.qty));;
            });

            $scope.LocalContext.Post('$scope.OutWards.items', $scope.OutWards.items, false);

            $scope.dgrid.UpdateData($scope.OutWards.items, '_id');
            $scope.sgrid.UpdateData($scope.SearchResults, '_id');
            $scope.sgrid.SetSelectedRows([]);
            $scope.dgrid.SetSelectedRows([]);
        };

        $scope.fileuploadedcomplete = function (value) {
            $scope.SearchCondition.fileid = null;
            $scope.excelfile = null;
            var barcodelist = [];
            $.each(value.rows, function (k, v) {
                barcodelist.push(v.values[0]);
                if (v.values.length > 1) {
                    var qty = parseFloat(v.values[1]);
                    if (typeof (qty) == 'undefined' || qty <= 0 || isNaN(qty))
                        qty = 1;
                    if (qty > 1) {
                        for (var i = 2; i <= qty; i++) {
                            barcodelist.push(v.values[0]);
                        }
                    }
                }
            });
            $scope.SearchStock(true, barcodelist.join(','), true);
        };

        $scope.RemoveItems = function () {
            var removeitem = $scope.sgrid.GetSelectedRows();
            $.each(removeitem, function (k, v) {
                $scope.SearchResults.splice($scope.SearchResults.indexOf(v), 1);
            });
            $scope.sgrid.UpdateData($scope.SearchResults, '_id');
            $scope.sgrid.SetSelectedRows([]);
        }

        $scope.PopItems = function () {
            var removeitem = [];
            $.each(this.dgrid.GetSelectedRows(), function (key, value) {
                $scope.SearchResults.push(value);
                removeitem.push(value);
            });

            $.each(removeitem, function (k, v) {
                $scope.OutWards.items.splice($scope.OutWards.items.indexOf(v), 1);
                v.selected = false;
            });

            $scope.TotalCounts = 0;
            $.each($scope.OutWards.items, function (k, v) {
                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);;
            });

            $scope.SearchCounts = 0;
            $.each($scope.SearchResults, function (k, v) {
                $scope.SearchCounts += (v.ismeter ? 1 : v.qty);;
            });
            $scope.dgrid.UpdateData($scope.OutWards.items, '_id');
            $scope.sgrid.UpdateData($scope.SearchResults, '_id');
            $scope.sgrid.SetSelectedRows([]);
            $scope.dgrid.SetSelectedRows([]);
        };

        $scope.SearchOutwords = {
            'fromwarehouseoutward': true, 'todate': null, 'fromdate': null, 'statusid': -1
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
            var value = $scope.LocalContext.Get('$scope.OutWards.items', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.OutWards.items = value;
            $scope.TotalCounts = 0;
            $.each($scope.OutWards.items, function (k, v) {
                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);;
            });

        };

        $scope.currentcompanyid = 0;
        $scope.SearchStock = function (barcoded, barcodelist, multiple) {

            if ($scope.Appsetting_Company &&
                (typeof ($scope.OutWards.companyid) == 'undefined' || $scope.OutWards.companyid <= 0)) {
                $rootScope.Toaster.Info(" Please select company");
                return;
            }

            if ($scope.currentcompanyid == 0) {
                $scope.currentcompanyid = $scope.OutWards.companyid;
            }
            else if ($scope.currentcompanyid != $scope.OutWards.companyid) {
                $scope.currentcompanyid = $scope.OutWards.companyid;
                $scope.SearchResults = [];
                $scope.OutWards.items = [];
            }

            if (barcoded && !multiple) {
                var existserialno = $scope.OutWards.items.find(function (itm) {
                    return itm.serialno == barcodelist;
                })
                if (existserialno && existserialno != null) {
                    var requireqty = 1;
                    for (var idx = 0; idx < $scope.OutWards.items.length; idx++) {
                        if ($scope.OutWards.items[idx].serialno != barcodelist) continue;
                        if ($scope.OutWards.items[idx].qty - $scope.OutWards.items[idx].iqty > 0) {
                            //$scope.OutWards.items[idx].qty += 1;
                            $scope.OutWards.items[idx].iqty += 1;
                            $scope.LocalContext.Post('$scope.OutWards.items', $scope.OutWards.items, false);
                            $scope.sgrid.UpdateData($scope.SearchResults, '_id');
                            $scope.dgrid.UpdateData($scope.OutWards.items, '_id');
                            $scope.SearchCondition.directbarcode = "";
                            return;
                        }
                    }
                    $rootScope.Toaster.Info("UAN items doesn't have enough stock, if having stock then create new bundle with current list as one bundle, create another one");
                    return;
                }
            }

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            if (barcoded) {
                request = {
                    "Item": { 'barcode': barcodelist, 'companyid': $scope.OutWards.companyid, 'searchtype': 5 },
                    "securitytoken": "String content"
                }
            }
            else {
                $scope.SearchCondition.companyid = $scope.OutWards.companyid;
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/SearchStock",
                data: srequest
            }).success(function (result) {

                if (angular.isArray(result)) {

                    $scope.SearchResults = [];

                    $.each($scope.OutWards.items, function (k, v) {
                        v.style = {};
                    });

                    //if (barcoded && !multiple) {
                    //    if (result.length > 1) {
                    //        result[0].stkqty = result[0].iqty;
                    //        result[0].qty = 1;
                    //        result[0].iqty = 1;
                    //        for (var i = 1; i < result.length; i++) {
                    //            result[i].stkqty = result[i].iqty;
                    //            result[i].qty = 0;
                    //            result[i].iqty = 0;
                    //        }
                    //    }
                    //}
                    var scannedqty = 0;
                    $.each(result, function (k, v) {

                        if (barcoded) {
                            var itm = $scope.OutWards.items.find(function (itm) {
                                return v.stockid == itm.stockid;
                            });
                            if (typeof (itm) == 'undefined' || itm == null) {
                                if (result.length > 1)
                                    $scope.OutWards.items.push(v);
                                else
                                    $scope.OutWards.items.splice(0, 0, v);
                                $scope.SearchCondition.directbarcode = "";
                                if (!v.ismeter && $scope.OutWards.multiscan) {
                                    if (scannedqty == 0) {
                                        v.iqty = 1;
                                        scannedqty++;
                                    }
                                    else
                                        v.iqty = 0;
                                }
                                v.style = { "background-color": "lightgrey" };
                                $scope.LocalContext.Post('$scope.OutWards.items', $scope.OutWards.items, false);
                            }
                            else {
                                if (!v.ismeter && $scope.OutWards.multiscan) {
                                    itm.iqty = $rootScope.GetNumber(itm.iqty) + 1;
                                }
                                else {
                                    itm.qty = v.qty;
                                    itm.iqty = v.iqty;
                                }
                                itm.style = { "background-color": "lightgrey" };
                                $scope.SearchCondition.directbarcode = "";
                                $scope.LocalContext.Post('$scope.OutWards.items', $scope.OutWards.items, false);
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


                $scope.SearchCounts = 0;
                $scope.TotalCounts = 0;
                $.each($scope.SearchResults, function (k, v) {
                    $scope.SearchCounts += (v.ismeter ? 1 : v.qty);
                });
                $scope.CalculateTotal();

                $scope.sgrid.UpdateData($scope.SearchResults, '_id');
                $scope.dgrid.UpdateData($scope.OutWards.items, '_id');

            }).error(function (result) {
            });
        }

        $scope.ConfigGrid = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('sourcegrid', '400px', '100%', true, true, true, 135);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('barcode', 'Barcode', 90);
            igrid.AddSimpleColumn('printingname', 'Name', 140);
            igrid.AddSimpleColumn('sizename', 'Size', 50);
            igrid.AddSimpleColumn('suppliername', 'Supplier', 100);
            igrid.AddSimpleColumn('lrreference', 'LR.Ref', 60);

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view)
                igrid.AddFormatedColumn('buyingprice', 'Cost', 60, 'N0', 2);

            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N0', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.sgrid = igrid;


            if ($scope.dgrid != null)
                $scope.dgrid.destroy();

            var igrid = new InfyGrid('destinationgrid', '400px', '100%', true, true, true, 135);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('barcode', 'Barcode', 90);
            igrid.AddSimpleColumn('printingname', 'Name', 100);
            igrid.AddSimpleColumn('sizename', 'Size', 50);
            igrid.AddSimpleColumn('suppliername', 'Supplier', 140);
            igrid.AddSimpleColumn('lrreference', 'LR.Ref', 100);

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view)
                igrid.AddFormatedColumn('buyingprice', 'Cost', 60, 'N0', 2);

            igrid.AddFormatedColumn('qty', 'Qty', 50, 'N0', 2).total = true;
            var column = igrid.AddColumn(
                {
                    id: "iqty", name: "Accepted", field: "iqty",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            )
            column.cssClass = 'rightalign';
            column.total = true;
            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.dgrid = igrid;
        }

        $scope.sgrid = null;
        $scope.dgrid = null;

        $scope.CalculateTotal = function () {
            $scope.TotalCounts = 0;
            $.each($scope.OutWards.items, function (k, v) {
                $scope.TotalCounts += (v.ismeter ? 1 : $rootScope.GetNumber(v.iqty));
            });
        }

        $scope.selectAllItems = function (items, mode) {

            $.each(items, function (key, value) {
                value.selected = mode;
            });

            /*if ($scope.OutWards.selectAll) {
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
            $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResults = [];
            $scope.OutWards.items = [];
            $scope.TotalAccepted = 0;
            $scope.TotalCounts = 0;
            if ($scope.sgrid != null)
                $scope.sgrid.UpdateData($scope.SearchResults, '_id');
            if ($scope.dgrid != null)
                $scope.dgrid.UpdateData($scope.OutWards.items, '_id');
        };

        $scope.Save = function () {
            var iserror = false;
            var wrongbarcode = '';
            $scope.TotalCounts = 0;
            $.each($scope.OutWards.items, function (k, v) {
                if (v.serialno == undefined || v.serialno == null || v.serialno == '') {
                    if (v.qty < v.iqty || v.iqty <= 0) {
                        wrongbarcode = v.barcode;
                        iserror = true;
                    }
                }
            });

            if (iserror) {
                $rootScope.ShowAlert('No data found', 'Wrong Out qty for - ' + wrongbarcode);
                return;
            }

            if ($scope.OutWards.packedon == null) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info(" Please select date");
                return;
            }
            if ($scope.OutWards.companyid == null || $scope.OutWards.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select compnay name");
                return;
            }

            if ($scope.OutWards.packedby == null || $scope.OutWards.packedby == 0) {
                $scope['setFocuspackedby']();
                $rootScope.Toaster.Info("Please select packedby");
                return;
            }
            if ($scope.OutWards.fromlocationid == null || $scope.OutWards.fromlocationid == 0) {
                $scope['setFocusfromlocationid']();
                $rootScope.Toaster.Info("Please select fromlocation");
                return;
            }

            if ($scope.OutWards.tolocationid == null || $scope.OutWards.tolocationid == 0) {
                $scope['setFocustolocationid']();
                $rootScope.Toaster.Info("Please select tolocation");
                return;
            }

            $scope.OutWards.inward2warehouse = true;

            request = {
                "Item": $scope.OutWards,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/Outward",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.LocalContext.Post('$scope.OutWards.items', null, false);
                    $rootScope.Toaster.Info("OutWards Saved Successfully.!");
                    $scope.OutWards.code = result.code;
                }
                else {
                    $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {
            });
        }

        $scope.Search = function () {
            $scope.SearchOutwords.fromwarehouseoutward = true;

            if ($scope.SearchOutwords.mode == '2') {
                $scope.SearchOutwords.fromwarehouseoutward = false;
                $scope.SearchOutwords.fromshowroom = true;
            }

            $scope.SearchOutwords.validatefromlocation = true;
            request = {
                "Item": $scope.SearchOutwords,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/Search",
                data: srequest
            }).success(function (result) {
                $scope.selectedcount = 0;
                $scope.SearchOutwardResult = result;

                $.each(result, function (k, v) {

                    v.cansave = false;
                    v.candelete = $scope.OutWards.status == 0
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

                $scope.ShowSearchGrid(result);

            }).error(function (result) {
            });
        }

        $scope.ShowSearchGrid = function (result) {
            if ($scope.searchgrid != null)
                $scope.searchgrid.destroy();

            var igrid = new InfyGrid('searchgrid', '400px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddFormatedColumn('packedon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('fromlocationid', 'From', 100, $scope.AllLocation);
            igrid.AddReferenceColumn('tolocationid', 'To', 100, $scope.AllLocation);
            igrid.AddFormatedColumn('qty', 'qty', 80, 'N0', 2).total = true;
            igrid.AddSimpleColumn('packedbyemployee', 'Packed by', 100);
            igrid.AddSimpleColumn('createdbyemployee', 'Created by', 100);
            igrid.AddSimpleColumn('statusdisplay', 'Status', 100);
            igrid.AddSimpleColumn('ewb', 'Eway', 100);
            var allowedit = true;
            if ($scope.SearchOutwords &&
                $scope.SearchOutwords.fromshowroom)
                allowedit = false;

            var commands = "";
            if ($scope.UserInfo.p_OUTWARD.modify && allowedit)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($scope.UserInfo.p_OUTWARD.delete && allowedit)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Delete'/> &nbsp;"
            if ($scope.UserInfo.p_OUTWARD.print) {
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Transport'/> &nbsp;"
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Print'/> &nbsp;"
            }
            //commands += "<input type='button' class='btn btn-danger btn-xs' value='Validate'/>"

            if (commands != '') {
                igrid.AddButtonColumn('action', 'ACTION', 170, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Transport')
                            $scope.ShowTransport(args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.PrintPDF(args.item);

                    });
                });
            }

            igrid.EnableTotal();
            igrid.Initialize();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.UpdateData(result, '_id');
            $scope.searchgrid = igrid;
        }

        $scope.Merge = function () {
            var rows = this.searchgrid.GetSelectedRows();
            var isvalid = true;
            var fromlocationid = rows[0].fromlocationid;
            var tolocationid = rows[0].tolocationid;
            var fromcompanyid = rows[0].fromcompanyid;
            var tocompanyid = rows[0].tocompanyid;
            var ids = [];
            $.each(rows, function (k, v) {
                if (v.status != 0 ||
                    fromlocationid != v.fromlocationid ||
                    tolocationid != v.tolocationid ||
                    fromcompanyid != v.fromcompanyid ||
                    tocompanyid != v.tocompanyid)
                    isvalid = false;

                ids.push(v.id);
            })
            if (!isvalid) {
                $rootScope.Toaster.Info("Only new bundles and Same From/To location can be merged");
                return;
            }
            var request = {
                "Item": {
                    'ids': ids
                },
                "securitytoken": "String content"
            }
            request.Item.validatefromlocation = true;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/Merge",
                data: srequest
            }).success(function (result) {
                $scope.searchgrid.UpdateData([result], '_id');
            }).error(function (result) {
            });
        }

        $scope.SearchByCode = function () {

            var request = {
                "Item": {
                    'code': $scope.OutWards.code,
                    'fromwarehouseoutward': true
                },
                "securitytoken": "String content"
            }

            request.Item.validatefromlocation = true;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/Search",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.LoadOutward(result[0]);
                }
                else {
                    $scope.OutWards.statusdisplay = 'Not Found!';
                }
            }).error(function (result) {
            });
        }

        $scope.LoadOutward = function (item) {
            $scope.OutWards = item;
            $scope.OutWards.cansave = false;
            $scope.OutWards.candelete = $scope.OutWards.status == 0
            $scope.OutWards.statusdisplay = '';
            $scope.OutWards.canreject = false;
            switch ($scope.OutWards.status) {
                case 0:
                    $scope.OutWards.statusdisplay = 'New Bundle';
                    break;
                case 1:
                    $scope.OutWards.statusdisplay = 'Inwarded Bundle';
                    break;
                case 2:
                    $scope.OutWards.statusdisplay = 'Inwarded Bundle with changes';
                    break;
                case 9:
                    $scope.OutWards.statusdisplay = 'Rejected Bundle';
                    break;
            }

            if ($scope.OutWards.status == 9)
                $scope.OutWards.id = 0;

            $scope.TotalCounts = 0;
            $scope.TotalAccepted = 0;
            $.each($scope.OutWards.items, function (k, v) {
                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
                if (v.aqty > 0)
                    $scope.TotalAccepted += (v.ismeter ? 1 : v.aqty);
            });
            $scope.Navigation.SetMode('EDIT');
            $scope.ShowSearch = false;
            $scope.dgrid.UpdateData($scope.OutWards.items, '_id');
        }


        $scope.ShowTransport = function (item) {
            var dialog = ngDialog.open({
                template: 'Transport',
                data: [$scope, item],
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.entity = $scope.ngDialogData[1];
                    $scope.Parent = $scope.ngDialogData[0];

                    if (typeof ($scope.entity.billimg) != 'undefined' &&
                        $scope.entity.billimg != null && $scope.entity.billimg != '') {
                        $scope.imagelist = $scope.entity.billimg.split(',');
                    }
                    else {
                        $scope.imagelist = [];
                    }

                    if ($scope.entity.lrdate == undefined || $scope.entity.lrdate == null
                        || $scope.entity.lrdate == '0001-01-01T00:00:00') {
                        $scope.entity.lrdate = new Date();
                    }

                    $scope.SupplyTypes = [
                        { 'id': 1, 'name': 'Supply' },
                        { 'id': 2, 'name': 'Import' },
                        { 'id': 3, 'name': 'Export' },
                        { 'id': 4, 'name': 'Job Work' },
                        { 'id': 5, 'name': 'Own Use' },
                        { 'id': 6, 'name': 'Job work Returns' },
                        { 'id': 7, 'name': 'Sales Return' },
                        { 'id': 8, 'name': 'Others' },
                        { 'id': 9, 'name': 'SKD/CKD/Lots' },
                        { 'id': 10, 'name': 'Line Sales' },
                        { 'id': 11, 'name': 'Recipient Not Known' },
                        { 'id': 12, 'name': 'Exhibition or Fairs' }
                    ];
                    $scope.TransportModes = [{ 'id': 1, 'name': 'Road' }, { 'id': 2, 'name': 'Rail' }, { 'id': 3, 'name': 'Air' }, { 'id': 4, 'name': 'Ship' }];

                    $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
                        $scope.Transports = results;
                    });
                    $scope.BillImageChanged = function (imageid) {
                        if (typeof (imageid) != 'undefined') {
                            if (angular.isArray(imageid)) {
                                if (imageid[0] != null && imageid[0] != '') {
                                    $scope.imagelist.splice(0, 0, imageid[0]);
                                }
                            }
                            else if (imageid != null && imageid != '') {
                                $scope.imagelist.splice(0, 0, imageid);
                            }
                        }
                        else {
                            $scope.imagelist.splice(0, 0, $scope.billimg);
                        }
                        $scope.billimg = null;
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }

                    $scope.RemoveImage = function (item) {
                        $scope.imagelist.splice($scope.imagelist.indexOf(item), 1);
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }

                    restService.Get('BundlesService/Transport/' + $scope.entity.id, true).then(function (results) {
                        if (results && results != null) {
                            $scope.entity = results;

                            if ($scope.entity.lrdate == undefined || $scope.entity.lrdate == null
                                || $scope.entity.lrdate == '0001-01-01T00:00:00') {
                                $scope.entity.lrdate = new Date();
                            }

                            if (typeof ($scope.entity.billimg) != 'undefined' &&
                                $scope.entity.billimg != null && $scope.entity.billimg != '') {
                                $scope.imagelist = $scope.entity.billimg.split(',');
                            }
                            else {
                                $scope.imagelist = [];
                            }
                        }
                    });

                    $scope.Selected = function () {

                        request = {
                            "Item": $scope.entity,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BundlesService/Transport",
                            data: srequest
                        }).success(function (result) {

                            if (result) {
                                $rootScope.Toaster.Info("Successfully Generated");
                                $scope.entity.ewb = results.ewb;
                                $scope.closeThisDialog(null);
                            }
                        }).error(function (result) {
                        });

                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                    }
                }
            });
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
                    url: "BundlesService/Delete",
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
                //$scope.LoadOutward(item);
                $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
                $scope.OutWards.code = item.code;
                $scope.SearchByCode();
            }
        }


        $scope.Delete = function () {
            request = {
                "Item": $scope.OutWards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/Delete",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.statusdisplay = "Successfully Removed";
                    $scope.ErrorStyle = "success";
                    $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
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
                "Item": $scope.OutWards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/Reject",
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
                var titlele = angular.element('#apptitle').context.title = "OUTWARDS SLIP";

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
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'Outward');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "outward.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }


        $scope.PrintPDF = function (item) {
            if (item && item != null)
                $rootScope.Download('BundlesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id, 'outward.pdf', 'application/pdf');
            else
                $rootScope.Download('BundlesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.OutWards.id, 'outward.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "debitnote.pdf";
                link.href = 'BundlesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.OutWards.id;
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
                "Item": { 'id': $scope.OutWards.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/Print",
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

        $scope.Appsetting_Company = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
                $scope.Appsetting_Company = true;
                $scope.intercompanytransfer = false || $rootScope.UserInfo.appsettings.system_interstatetransfer;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }


        $timeout(function () {
            $scope.ConfigGrid();
            $scope.LoadApplicationSetting();
            $scope.setFocusfirst();
        }, 10);
        $scope.CreateNew = function () {
            $scope.OutWards.packedon = new Date();
        }();
    }]);
