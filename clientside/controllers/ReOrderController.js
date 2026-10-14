
var app = angular.module('InfinityLab');

app.controller('ReOrderController', ['UserDetails', '$scope', '$http', '$location', 'ngDialog', '$rootScope', '$window', 'restService', '$timeout', '$stateParams', '$filter', 'cntxService', '$state',
    function (UserDetails, $scope, $http, $location, ngDialog, $rootScope, $window, restService, $timeout, $stateParams, $filter, cntxService, $state) {
        $scope.SearchRef = restService.SearchReference;
        $scope.IsReport = $stateParams.type == 'REPORT';
        $scope.IsOnCheck = false;
        $scope.ShowReport = $scope.IsReport;
        $scope.grid = null;
        $scope.SupportAttributeFilter = false;
        $scope.ReorderResult = [];
        $scope.DisplayMode = 'Summary';
        $scope.DisplayModeList = [
            { 'id': 'Summary', 'name': 'Summary' },
            { 'id': 'Detail', 'name': 'Detail' },
            { 'id': 'Matrix', 'name': 'Matrix' },
            { 'id': 'Upload', 'name': 'Upload' },
            { 'id': 'Change Matrix', 'name': 'Change Matrix' }];

        $scope.LoadSettings = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadSettings, 100);
            }
            else {
                $scope.SupportAttributeFilter = $rootScope.UserInfo.appsettings.system_supportattributefilter || false;
            }
        }

        $scope.LoadSettings();
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
            $scope.AllSupplier = results;
        });
        $scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Section = results;
        });
        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'name': 'All Warehouse Location', 'id': -1 });
            results.push({ 'name': 'All Retail Location', 'id': -2 });
            results.push({ 'name': 'All Stock', 'id': -3 });
        });
        $scope.Counter = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counter = results;
        });

        $scope.AllProduct = [];
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
            results.push({ 'name': 'All Core Products', 'id': -1 });
            results.push({ 'name': 'All Readymade Products', 'id': -2 });
            results.push({ 'name': 'None', 'id': 0 });
            $scope.AllProduct = results;
        });

        $scope.MarkerType = restService.Get('ReferencelistService/MARKER', true).then(function (results) {
            $scope.MarkerType = results;
        });

        $scope.PriorityList = [{ 'id': 0, 'name': 'Low' }, { 'id': 1, 'name': 'Medium' },
        { 'id': 2, 'name': 'High' }, { 'id': 3, 'name': 'Critical' }];


        $scope.CoreOptionList = [{ 'id': 1, 'name': 'All' }, { 'id': 2, 'name': 'Core' }, { 'id': 3, 'name': 'Non-Core' }];

        $scope.AllSizes = [];
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
            var exist = false;
            $.each($scope.Sizes, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.Sizes.push({ id: -2, name: 'Jump' });
                $scope.Sizes.push({ id: -3, name: 'Cut' });
            }
            $scope.AllSizes = results;
        });

        $scope.AllBrand = [];
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
            $scope.AllBrand = results;
        });

        $scope.AllPattern = [];
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
            $scope.AllPattern = results;
        });

        $scope.AllSleeve = [];
        $scope.Material = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.Sleeve = results;
            $scope.AllSleeve = results;
        });

        $scope.AllFit = [];
        $scope.Material = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.Fit = results;
            $scope.AllFit = results;
        });

        $scope.AllMaterial = [];
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
            $scope.AllMaterial = results;
        });

        $scope.AllPattern = [];
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
            $scope.AllPattern = results;
        });

        $scope.AllStyle = [];
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
            $scope.AllStyle = results;
        });

        $scope.AllColors = [];
        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
            $scope.AllColors = results;
        });

        $scope.TypeList = [];
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
            $scope.AllTypeList = results;
        });

        $scope.Entity = {
            "productid": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "rules": []
        };

        $scope.markeddeleted = [];
        $scope.ReOrder = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ruleEntity = {
            "id": 0, "productid": "", "supplierid": "", "brandid": "", "styleid": "", "patternid": "", "colorid": "",
            "materialid": "", "sizeid": "", "typeid": "", "fromrate": 0, "torate": 0, "minro": 0, "maxro": 0, "priority": "",
            "description": "", enable: false, "isactive": true, "minro": "", "maxro": "", "enable": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00"
        };

        $scope.productChange = function () {
            if ($scope.SupportAttributeFilter && $scope.ReOrder.productid != 'undefined'
                && $scope.ReOrder.productid != null) {
                var prditem = $scope.AllProduct.find(function (item) {
                    return (item.id == $scope.ReOrder.productid);
                });
                if (prditem || prditem != null) {
                    $scope.ProductAttributeFilter(prditem.attributelist);
                }
            }
        }
        $scope.ProductAttributeFilter = function (attributelist) {
            $scope.Style = $scope.FilterArray($scope.AllStyle, attributelist == null ? null : attributelist.style);
            $scope.Pattern = $scope.FilterArray($scope.AllPattern, attributelist == null ? null : attributelist.pattern);
            $scope.Material = $scope.FilterArray($scope.AllMaterial, attributelist == null ? null : attributelist.material);
            $scope.Sizes = $scope.FilterArray($scope.AllSizes, attributelist == null ? null : attributelist.size, [-2, -3, -4]);
            $scope.TypeList = $scope.FilterArray($scope.AllTypeList, attributelist == null ? null : attributelist.type);
            $scope.Colors = $scope.FilterArray($scope.AllColors, attributelist == null ? null : attributelist.colour);
        }
        $scope.FilterArray = function (sourcelist, filterlist, addlist) {
            if (filterlist == null || filterlist.length == 0) return sourcelist;

            var newlist = [];
            $.each(filterlist, function (k, v) {
                var item = sourcelist.find(function (item) {
                    return item.id == v;
                });
                if (item && item.id > 0)
                    newlist.push(item);
            });
            if (addlist && addlist.length > 0) {
                $.each(addlist, function (k, v) {
                    var item = sourcelist.find(function (item) {
                        return item.id == v;
                    });
                    if (item && (item.id > 0 || item.id == -2 || item.id == -3 || item.id == -4))
                        newlist.push(item);
                });
            }

            return newlist;
        }

        $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
        $scope.Convert2PO = function () {
            var rows = [];
            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                if (v.orderqty > 0) {
                    v.order = v.orderqty;
                    v.pricerange = v.aprice;
                    rows.push(v);
                }
            });
            $scope.LocalContext = cntxService.CreatePageContext('POBuilder');
            $scope.LocalContext.Post('OrderList', rows, false);
            $state.go('PurchaseOrder', {
                directlaunch: false,
                source: 'Reorder',
                locationid: $scope.ReOrder.locationid,
                list: rows
            });
        }

        $scope.ModeChanged = function () {
            if ($scope.DisplayMode == 'Change Matrix') {
                $scope.ChooseMatrix();
                return;
            }

            if ($scope.DisplayMode == 'Matrix') {
                $scope.ShowMatrixConfig();
                return;
            }
            if ($scope.ReOrder.productid != 'undefined'
                && $scope.ReOrder.productid != null && $scope.ReOrder.productid > 0) {
                $scope.ReportConfigGrid(150);
            }
            else {
                $scope.ReportConfigGrid(50);
            }
            $scope.cgrid.UpdateData($scope.ReOrder.rules);
        }

        $scope.Add = function () {

            if (typeof ($scope.ReOrder.rules) == 'undefined' || $scope.ReOrder.rules == null)
                $scope.ReOrder.rules = [];

            var array = [];

            if ($scope.rule.supplierid > 0)
                array.push('SUPLR:' + $scope.SearchRef($scope.rule.supplierid, $scope.Supplier));
            if ($scope.rule.sizeid > 0)
                array.push('SIZE:' + $scope.SearchRef($scope.rule.sizeid, $scope.Sizes));
            if ($scope.rule.styleid > 0)
                array.push('STYLE:' + $scope.SearchRef($scope.rule.styleid, $scope.Style));

            if ($scope.rule.brandid > 0)
                array.push('BRAND:' + $scope.SearchRef($scope.rule.brandid, $scope.Brand));
            if ($scope.rule.patternid > 0)
                array.push('PATRN:' + $scope.SearchRef($scope.rule.patternid, $scope.Pattern));
            if ($scope.rule.colourid > 0)
                array.push('COLOUR:' + $scope.SearchRef($scope.rule.colourid, $scope.Colors));
            if ($scope.rule.materialid > 0)
                array.push('MATRL:' + $scope.SearchRef($scope.rule.materialid, $scope.Material));
            if ($scope.rule.typeid > 0)
                array.push('TYPE:' + $scope.SearchRef($scope.rule.typeid, $scope.TypeList));

            if ($scope.rule.fromrate > 0)
                array.push('FROM RS:' + $scope.rule.fromrate);
            if ($scope.rule.torate > 0)
                array.push('TO RS:' + $scope.rule.torate);

            $scope.rule.productid = $scope.ReOrder.productid;
            $scope.rule.locationid = $scope.ReOrder.locationid;
            $scope.rule.description = array.length == 0 ? 'All ' + $scope.SearchRef($scope.rule.productid, $scope.Product) : array.join(',');
            $scope.rule.id = ($scope.ReOrder.rules.length + 1) * -1;
            $scope.ReOrder.rules.push($scope.rule);
            $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
            $scope.cgrid.UpdateData($scope.ReOrder.rules);
        };


        $scope.ManageItem = function (action, item) {
            if (action == 'Delete') {
                if (typeof (item.id) != 'undefined' && item.id > 0)
                    $scope.markeddeleted.push(item);
                $scope.ReOrder.rules.splice($scope.ReOrder.rules.indexOf(item), 1);
                $scope.cgrid.UpdateData($scope.ReOrder.rules);
            }
        }

        $scope.ErrorMessage = "";

        $scope.SearchRO = function (productid, locationid) {

            $scope.ReportConfigGrid(150);
            if (typeof (productid) == 'undefined' || productid == null || productid == 0) {
                $rootScope.Toaster.Info('Please select the product');
                return;
            }

            if (typeof (locationid) == 'undefined' || locationid == null || locationid == 0) {
                $rootScope.Toaster.Info('Please select the location');
                return;
            }



            request = {
                "Item": {
                    'productid': productid, 'locationid': locationid, 'supplierid': $scope.rule.supplierid, 'brandid': $scope.rule.brandid,
                    'materialid': $scope.rule.materialid, 'sizeid': $scope.rule.sizeid, 'colourid': $scope.rule.colourid, 'styleid': $scope.rule.styleid,
                    'patternid': $scope.rule.patternid, 'typeid': $scope.rule.typeid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReorderrulesService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.ReOrder.rules = result;
                }
                else {
                    $scope.ReOrder.rules = [];
                }
                $scope.markeddeleted = [];

                $scope.cgrid.UpdateData($scope.ReOrder.rules);
                $scope.productChange();
            }).error(function (result) {
            });
        }

        $scope.SearchCondition = {
            code: "",
            name: ""
        }

        $scope.exporttoExcel = function () {
            var infygrid = new InfyGrid();
            $scope.exportHref = infygrid.ExportExcel($scope.grid, 'reorder.xls');
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

        $scope.exportExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'STOCK RE-ORDER STATUS REPORT ON : ' + $filter('date')(new Date(), 'dd-MMM-yyyy');

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
        $scope.Back = function () {
            $scope.ShowReport = false;
        };

        $scope.ReportItems = function () {
            request = {
                "Item": {
                    'productid': $scope.ReOrder.productid, 'locationid': $scope.ReOrder.locationid,
                    'brandid': $scope.ReOrder.brandid, 'materialid': $scope.ReOrder.materialid,
                    'patternid': $scope.ReOrder.patternid, 'styleid': $scope.ReOrder.styleid,
                    'colourid': $scope.ReOrder.colorid, 'sizeid': $scope.ReOrder.sizeid,
                    'typeid': $scope.ReOrder.typeid, 'showall': $scope.ReOrder.showall,
                    'sectionid': $scope.ReOrder.sectionid, 'salerange': $scope.ReOrder.salerange,
                    'salefromdate': $scope.ReOrder.salefromdate, 'saletodate': $scope.ReOrder.saletodate
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $scope.ShowReport = true;
            $http({
                method: 'POST',
                url: "ReorderrulesService/ReportItems",
                data: srequest
            }).success(function (result) {
                $timeout(function () {
                    $scope.ReorderResult = result;
                    $scope.ReportGridConfig();
                }, 100);
            }).error(function (result) {
            });
        }

        $scope.Report = function (productid, locationid, checknow) {
            request = {
                "Item": {
                    'productid': $scope.ReOrder.productid, 'locationid': $scope.ReOrder.locationid,
                    'brandid': $scope.ReOrder.brandid, 'materialid': $scope.ReOrder.materialid,
                    'patternid': $scope.ReOrder.patternid, 'styleid': $scope.ReOrder.styleid,
                    'colourid': $scope.ReOrder.colorid, 'sizeid': $scope.ReOrder.sizeid,
                    'typeid': $scope.ReOrder.typeid, 'showall': $scope.ReOrder.showall,
                    'sectionid': $scope.ReOrder.sectionid, 'salerange': $scope.ReOrder.salerange,
                    'salefromdate': $scope.ReOrder.salefromdate, 'saletodate': $scope.ReOrder.saletodate
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            if (typeof (checknow) == 'undefined' || checknow == null)
                checknow = false;

            if (checknow)
                $scope.IsOnCheck = true;

            $scope.ShowReport = true;
            $http({
                method: 'POST',
                url: "ReorderrulesService/Report",
                data: srequest
            }).success(function (result) {

                $timeout(function () {
                    $scope.ReorderResult = result;
                    $scope.ReportGridConfig();
                }, 100);

                //$scope.ReportConfig(checknow, $scope.ReOrder.locationid);

                //var id = 0;
                //$.each(result, function (k, v) {
                //    v.id = id++;
                //    var product = $scope.Product.find(function (vv,kk) {
                //        return vv.id == v.productid;
                //    });
                //    if (typeof (product) != 'undefined' && product != null)
                //        v.product = product.name;

                //    var location = $scope.Location.find(function (vv, kk) {
                //        return vv.id == v.locationid;
                //    });
                //    if (typeof (location) != 'undefined' && location != null)
                //        v.location = location.name;
                //});

                //$scope.dataView.beginUpdate();
                //$scope.dataView.setItems(result);
                //$scope.dataView.endUpdate();

            }).error(function (result) {
            });
        }

        $scope.ReportConfig = function (checknow, locationid) {

            if ($scope.grid != null) {
                $scope.grid.destroy();
                $(checknow ? "#checkmyGrid" : "#myGrid").empty();
            }

            var columns = [];
            if ($scope.IsReport) {
                var checkboxSelector = new Slick.CheckboxSelectColumn({
                    cssClass: "slick-cell-checkboxsel"
                });
                checkboxSelector.width = 40;
                columns.push(checkboxSelector.getColumnDefinition());
            }
            columns.push({ id: "title", name: "Title", field: "title", width: 300, minWidth: 50, cssClass: "cell-title", sortable: true });

            if (!(typeof (locationid) != 'undefined' && locationid > 0))
                columns.push({ id: "location", name: "Location", field: "location", width: 100, minWidth: 50, sortable: true });

            columns.push({ id: "expect", name: "Stock Range", field: "expect", width: 120, minWidth: 50, sortable: true });
            columns.push({ id: "pricerange", name: "Price Range", field: "pricerange", width: 120, minWidth: 50, sortable: true });

            //if (typeof (locationid) != 'undefined' && locationid > 0) {
            columns.push({ id: "retailstock", name: "Stock", field: "retailstock", width: 100, minWidth: 50, sortable: true });
            columns.push({ id: "otherretailstock", name: "Other Store", field: "otherretailstock", width: 100, minWidth: 50, sortable: true });
            columns.push({ id: "stock", name: "Warehouse", field: "stock", width: 100, minWidth: 50, sortable: true });

            columns.push({ id: "shortage", name: "Shortage", field: "shortage", width: 100, minWidth: 50, sortable: true });
            columns.push({ id: "orderqty", name: "Order", field: "orderqty", width: 100, minWidth: 50, sortable: true });

            //}
            //else {
            //    columns.push({ id: "stock", name: "Stock", field: "stock", width: 100, minWidth: 50, sortable: true });
            //    columns.push({ id: "retailstock", name: "Store", field: "retailstock", width: 100, minWidth: 50, sortable: true });
            //}

            columns.push({ id: "status", name: "Priority", field: "status", width: 100, minWidth: 50, sortable: true });
            columns.push({ id: "stockstatus", name: "Status", field: "stockstatus", width: 100, minWidth: 50, sortable: true });
            var groupItemMetadataProvider = new Slick.Data.GroupItemMetadataProvider();
            dataView = new SlickEx.Data.DataView({
                groupItemMetadataProvider: groupItemMetadataProvider,
                inlineFilters: true
            });

            dataView.setGrouping({
                getter: "Product",
                formatter: function (g) {
                    return "Product:  " + g.value;
                },
                aggregateCollapsed: false,
                lazyTotalsCalculation: false
            });

            var options = {
                enableCellNavigation: true,
                editable: true
            };

            grid = new SlickEx.Grid(checknow ? "#checkmyGrid" : "#myGrid", dataView, columns, options);
            grid.registerPlugin(groupItemMetadataProvider);
            //grid.setSelectionModel(new Slick.CellSelectionModel());


            // wire up model events to drive the grid
            dataView.onRowCountChanged.subscribe(function (e, args) {
                grid.updateRowCount();
                grid.render();
            });
            dataView.onRowsChanged.subscribe(function (e, args) {
                grid.invalidateRows(args.rows);
                grid.render();
            });

            $scope.dataView = dataView;
            $scope.grid = grid;
        }

        $scope.Select2Formatter = function (row, cell, value, columnDef, dataContext) {
            return columnDef.dataSource[value] || '-';
        }

        $scope.ReportConfigGrid = function (top, isupload) {

            if ($scope.cgrid != null)
                $scope.cgrid.destroy();
            var cgrid = new InfyGrid('configContainer', '470px', '100%', true, true, true, top);
            cgrid.options.showHeaderRow = true;
            if ($scope.DisplayMode == 'Summary') {
                cgrid.AddSimpleColumn('description', 'DESCRIPTION', 350);
            }
            else {
                cgrid.AddReferenceColumn('supplierid', 'Supplier Name', 120, $scope.Supplier);
                if (isupload)
                    cgrid.AddReferenceColumn('productid', 'Product', 120, $scope.Product);
                cgrid.AddReferenceColumn('brandid', 'Brand', 80, $scope.Brand);
                cgrid.AddReferenceColumn('patternid', 'Pattern', 80, $scope.Pattern);
                cgrid.AddReferenceColumn('styleid', 'Style', 80, $scope.Style);
                cgrid.AddReferenceColumn('colourid', 'Colour', 80, $scope.Colors);
                cgrid.AddReferenceColumn('materialid', 'Material', 80, $scope.Material);
                cgrid.AddReferenceColumn('typeid', 'Type', 80, $scope.TypeList);
            }
            cgrid.AddReferenceColumn('sizeid', 'Size', 60, $scope.Sizes);
            var priority = cgrid.AddSimpleColumn('priority', 'Priority', 100);
            priority.formatter = $scope.Select2Formatter;
            priority.editor = Slick.Editors.Select2Editor;
            priority.dataSource = { 0: 'Low', 1: 'Medium', 2: 'High', 3: 'Critical' };
            cgrid.AddSimpleColumn('designid', 'DESIGN', 100).editor = Slick.Editors.Text;
            cgrid.AddSimpleColumn('fromrate', 'From Price', 80).editor = Slick.Editors.NumberEditor;
            cgrid.AddSimpleColumn('torate', 'To Price', 80).editor = Slick.Editors.NumberEditor;
            cgrid.AddSimpleColumn('minro', 'MIN Qty', 80).editor = Slick.Editors.NumberEditor;
            cgrid.AddSimpleColumn('maxro', 'MAX Qty', 80).editor = Slick.Editors.NumberEditor;
            cgrid.AddSimpleColumn('expirydays', 'DAYS', 80).editor = Slick.Editors.NumberEditor;
            cgrid.AddSimpleColumn('aprice', 'A.PRICE', 80).editor = Slick.Editors.NumberEditor;
            cgrid.AddSimpleColumn('minorder', 'M.ORDER', 80).editor = Slick.Editors.NumberEditor;

            cgrid.AddCustomColumn('enable', '<input type="checkbox" style="z-index: auto;margin-left:5px">', 40, function (item, col) {
                if (item.enable)
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
            });


            commands = "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            cgrid.AddButtonColumn('action', 'ACTION', 80, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Delete')
                        $scope.ManageItem('Delete', args.item);
                });
            });
            cgrid.options.autoEdit = true;
            cgrid.Initialize();

            cgrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'enable' && $(e.target).is(":checkbox")) {
                    if (cgrid.grid.getEditorLock().isActive() && !cgrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }

                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < cgrid.grid.getData().getLength(); i++) {
                            var item = cgrid.grid.getDataItem(i);
                            item.enable = true;
                        }

                    } else {
                        for (var i = 0; i < cgrid.grid.getData().getLength(); i++) {
                            var item = cgrid.grid.getDataItem(i);
                            item.enable = false;
                        }
                    }
                    cgrid.grid.invalidate();
                    cgrid.grid.render();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                }
            });

            cgrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'enable') {
                    item = args.grid.getDataItem(args.row);
                    item.enable = !item.enable && $(e.target).val() == 'on';
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });
            $scope.cgrid = cgrid;
        }

        $scope.ChooseMatrix = function (ItemDetails, callback) {
            var dialog = ngDialog.open({
                template: 'ChooseMatrix',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', 'restService',
                    function ($scope, $timeout, restService) {
                        $scope.ParentScope = $scope.ngDialogData.scope;
                        $scope.config = {};
                        $scope.ConfigRow = null;
                        $scope.ColumnList = [
                            { 'id': 'Brand', 'name': 'Brand' }, { 'id': 'Size', 'name': 'Size' },
                            { 'id': 'Style', 'name': 'Style' }, { 'id': 'Fit', 'name': 'Fit' },
                            { 'id': 'Sleeve', 'name': 'Sleeve' }, { 'id': 'Type', 'name': 'Type' },
                            { 'id': 'Colour', 'name': 'Colour' }, { 'id': 'Material', 'name': 'Material' },
                            { 'id': 'Pattern', 'name': 'Pattern' }, { 'id': 'Type', 'name': 'Type' },
                            { 'id': 'Price Range', 'name': 'Price Range' }, { 'id': 'Location', 'name': 'Location' }];

                        var product = $scope.ParentScope.Product.find(function (item) {
                            return item.id == $scope.ParentScope.ReOrder.productid;
                        });

                        restService.Get('ReferencelistService/DIRECT-REORDER/' + product.id, true).then(function (results) {
                            if (results.length > 0) {
                                $scope.ConfigRow = results[0];
                                $scope.config = JSON.parse(results[0].data);
                            }
                        });

                        $scope.Selected = function () {

                            if ($scope.ConfigRow == null) {
                                $scope.ConfigRow = {
                                    'isactive': true, 'type': 'REORDER',
                                    'code': 'REORDER'
                                };
                            }

                            $scope.ConfigRow.name = product.name;
                            $scope.ConfigRow.productgroup = product.id;
                            $scope.ConfigRow.data = JSON.stringify($scope.config);
                            var request = {
                                "Item": $scope.ConfigRow,
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "ReferencelistService/SaveDirect",
                                data: srequest
                            }).success(function (result) {

                                if (result != null && result.id > 0) {
                                    $rootScope.Toaster.Info("Saved Successfully.!");
                                    $scope.closeThisDialog($scope.Config);
                                }

                            }).error(function (result) {
                            });

                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.DisplayMode = '';
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        $scope.DisplayMode = 'Matrix';
                    }
                }
            });
        }

        $scope.ShowMatrixConfig = function () {
            $scope.MatrixConig = null;
            var product = $scope.Product.find(function (item) {
                return item.id == $scope.ReOrder.productid;
            });

            restService.Get('ReferencelistService/DIRECT-REORDER/' + product.id, true).then(function (results) {
                if (results.length > 0) {
                    $scope.SearchMatrixConfig(JSON.parse(results[0].data));
                }
            });
        }

        $scope.SearchMatrixConfig = function (config) {

            var request = {
                "Item": {
                    'productid': $scope.ReOrder.productid, 'locationid': $scope.ReOrder.locationid,
                    'matrixconfig': config
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReorderrulesService/ReorderMatrix",
                data: srequest
            }).success(function (result) {
                $scope.BuildMatrixGrid(config, result);
            });
        }

        $scope.RecursiveRule = function (frows, rows, name, addkey) {
            var rowslength = rows.length;
            var frowslength = frows.length;
            var finalrows = [];
            for (var f = 0; f < frowslength; f++) {
                for (var r = 0; r < rowslength; r++) {
                    var row = (JSON.parse(JSON.stringify(frows[f])));
                    row[name + 'id'] = rows[r].id;
                    if (name == 'Price') {
                        row['fromrate'] = rows[r].fromrate;
                        row['torate'] = rows[r].torate;
                    }
                    if (addkey) {
                        if (typeof (row['key']) == 'undefined')
                            row['key'] = '' + rows[r].id;
                        else {
                            row['key'] += '-' + rows[r].id;
                        }
                    }
                    finalrows.push(row);
                }
            }
            return finalrows;
        }

        $scope.BuildMatrixGrid = function (config, data) {
            var finalrows = [{
                'minro': 0,
                'productid': $scope.ReOrder.productid,
                'locationid': $scope.ReOrder.locationid
            }];

            $.each(data.DataSource, function (k, v) {
                finalrows = $scope.RecursiveRule(finalrows, v.rows, v.name, v.name.toUpperCase() != config.column.toUpperCase());
            });

            if (data.PriceRanges != null && data.PriceRanges.length > 0) {
                $.each(data.PriceRanges, function (k, v) {
                    v.id = '' + v.from + '~' + v.to;
                    v.name = '' + v.from + '-' + v.to;
                    v.fromrate = v.from;
                    v.torate = v.to;
                });
                finalrows = $scope.RecursiveRule(finalrows, data.PriceRanges, 'Price', true);
            }

            var columnkey = config.column.toLowerCase() + 'id';
            var grouprows = [];
            $.each(finalrows, function (k, v) {
                var exist = grouprows.find(function (item) {
                    return item.key == v.key;
                });

                if (exist && exist != null) {
                    exist['c_' + v[columnkey]] = v.minro;
                    exist.rows.push(v);
                }
                else {
                    var row = (JSON.parse(JSON.stringify(v)));
                    row.rows = [];
                    row.column = columnkey;
                    row.rows.push(v);
                    row['c_' + v[columnkey]] = v.minro;
                    grouprows.push(row);
                }
            });

            $scope.ReOrder.rules = data.Rules;

            if (data.Rules != null && data.Rules.length > 0) {
                $.each(data.DataSource, function (k, v) {
                    if (v.name.toUpperCase() != config.column.toUpperCase()) {
                        var key = v.name + 'id';
                        $.each(data.Rules, function (ek, ev) {
                            if (typeof (ev.key) == 'undefined')
                                ev.key = '' + ev[key];
                            else
                                ev.key += '-' + ev[key];
                        });
                    }
                });

                if (data.PriceRanges != null && data.PriceRanges.length > 0) {
                    var key = 'Priceid';
                    $.each(data.Rules, function (ek, ev) {
                        if (typeof (ev.key) == 'undefined')
                            ev.key = '' + ev.fromrate + '~' + ev.torate;
                        else
                            ev.key += '-' + ev.fromrate + '~' + ev.torate;
                    });
                    finalrows = $scope.RecursiveRule(finalrows, data.PriceRanges, 'Price', true);
                }

                $.each(data.Rules, function (k, v) {
                    var exist = grouprows.find(function (item) {
                        return item.key == v.key;
                    });
                    if (exist && exist != null) {
                        var columnid = v[columnkey];
                        exist['c_' + columnid] = v.minro;

                        var rexist = exist.rows.find(function (item) {
                            return item[columnkey] == v[columnkey];
                        });
                        if (rexist && rexist != null) {
                            rexist.minro = v.minro;
                            rexist.id = v.id;
                            rexist.version = v.version;
                            rexist.modifiedby = v.modifiedby;
                            rexist.createdby = v.createdby;
                            rexist.modifiedon = v.modifiedon;
                            rexist.createdon = v.createdon;
                        }
                    }
                })
            }





            $scope.MatrixRows = grouprows;
            if ($scope.cgrid != null)
                $scope.cgrid.destroy();
            var cgrid = new InfyGrid('configContainer', '470px', '100%', true, true, true, 75);
            cgrid.options.showHeaderRow = true;
            cgrid.EnableSerial($scope.MatrixRows, true);

            var columnsource = null;
            $.each(data.DataSource, function (k, v) {
                if (v.name.toUpperCase() != config.column.toUpperCase()) {
                    cgrid.AddReferenceColumn(v.name + 'id', v.name, 120, v.rows);
                }
                else {
                    columnsource = v;
                }
            });

            if (data.PriceRanges != null && data.PriceRanges.length > 0) {
                cgrid.AddReferenceColumn('Priceid', 'Price', 120, data.PriceRanges);
            }

            $.each(columnsource.rows, function (k, v) {
                var column = cgrid.AddColumn(
                    {
                        id: "c_" + v.id, name: v.name, field: "c_" + v.id,
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.Internal = v;
            });

            cgrid.options.autoEdit = true;
            cgrid.Initialize();

            cgrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var column = args.grid.getColumns()[args.cell];
                var row = item.rows.find(function (sitem) {
                    return sitem[item.column] == column.Internal.id;
                });
                if (row && row != null) {
                    row.minro = item[column.field];
                }
                $timeout(function () {
                    args.grid.invalidateRow(args.row);
                    args.grid.render();
                }, 10);

            });
            $scope.cgrid = cgrid;
            $scope.cgrid.UpdateData($scope.MatrixRows);
        }

        $scope.SaveMatrix = function () {

            if ($scope.ReOrder.rules == null)
                $scope.ReOrder.rules = [];

            var uid = 1;
            $.each($scope.MatrixRows, function (k, v) {
                $.each(v.rows, function (kk, vv) {
                    if (vv.id && vv.id > 0) {
                        var exist = $scope.ReOrder.rules.find(function (item) {
                            return item.id == vv.id;
                        });
                        if (exist && exist != null) {
                            exist.minro = vv.minro;
                        }
                    }
                    else {
                        if (vv.minro && vv.minro > 0) {
                            vv.uid = uid++;
                            $scope.ReOrder.rules.push(vv);
                        }
                    }
                });
            });

            var request = {
                "Item": $scope.ReOrder,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ReorderrulesService/SaveAll",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Re Order Saved Successfully.!");

                $.each(result.rules, function (k, v) {

                    var old = $scope.ReOrder.rules.find(function (item) {
                        return item.uid == v.uid;
                    });

                    if (old && old != null) {
                        old.id = v.id;
                        old.version = v.version;
                        old.modifiedby = v.modifiedby;
                        old.createdby = v.createdby;
                        old.modifiedon = v.modifiedon;
                        old.createdon = v.createdon;
                    }

                })

            }).error(function (result) {
            })
        }

        $scope.ReportGridConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.ReorderResult);

            igrid.AddReferenceColumn('locationid', 'Location Name', 120, $scope.Location);
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 120, $scope.Supplier);
            igrid.AddReferenceColumn('productid', 'Product', 120, $scope.Product);
            igrid.AddReferenceColumn('brandid', 'Brand', 80, $scope.Brand);
            igrid.AddReferenceColumn('patternid', 'Pattern', 80, $scope.Pattern);
            igrid.AddReferenceColumn('styleid', 'Style', 80, $scope.Style);
            igrid.AddReferenceColumn('colourid', 'Colour', 80, $scope.Colors);
            igrid.AddReferenceColumn('materialid', 'Material', 80, $scope.Material);
            igrid.AddReferenceColumn('typeid', 'Type', 80, $scope.TypeList);
            igrid.AddReferenceColumn('sizeid', 'Size', 60, $scope.Sizes);
            igrid.AddSimpleColumn('designid', 'Design No', 80);
            igrid.AddSimpleColumn('expect', 'Stock Range', 100);
            igrid.AddSimpleColumn('pricerange', 'Price Range', 100);
            igrid.AddSimpleColumn('retailstock', 'Stock', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('otherretailstock', 'Other Store', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('stock', 'Warehouse', 80, 'N2', 2).total = true;

            igrid.AddSimpleColumn('shortage', 'Shortage', 60, 'N2', 2).total = true;
            igrid.AddSimpleColumn('excess', 'Excess', 60, 'N2', 2).total = true;
            igrid.AddSimpleColumn('orderqty', 'Order Qty', 60, 'N2', 2).total = true;

            if ($scope.ReOrder.salerange) {
                igrid.AddSimpleColumn($scope.ReOrder.locationid > 0 ? 'sold' : 'tsold', 'Sold Qty', 80);
            }

            igrid.AddSimpleColumn('status', 'Priority', 80);
            igrid.AddSimpleColumn('stockstatus', 'Status', 80);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.ReorderResult);
        }

        $scope.GetID = function (index, valuearray, reflist) {
            if (index >= 0 && valuearray[index] && valuearray[index] != null && valuearray[index] != '') {
                if (reflist && reflist != null && reflist.length > 0) {
                    var item = reflist.find(function (itm) {
                        return itm.name == valuearray[index];
                    });
                    if (item && item != null) {
                        return item.id;
                    }
                    return 0;
                }
                else if (valuearray[index].indexOf('-') > 0) {
                    return valuearray[index].split('-');
                }
                return valuearray[index];
            }
            return null;
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.stockadjustmentexcel = null;
            var csupplier = -1;
            var cbrand = -1;
            var cproduct = -1;
            var csize = -1;
            var ccolour = -1;
            var cpattern = -1;
            var cstyle = -1;
            var ctype = -1;
            var cmaterial = -1;
            var csleeve = -1;
            var cfit = -1
            var cstockrange = -1;
            var cminorder = -1;
            var cprice = -1;
            var cdesign = -1;
            var cpricerange = -1;
            if ($scope.ReOrder.locationid == 'undefined' || $scope.ReOrder.locationid == null || $scope.ReOrder.locationid == 0) {
                $rootScope.Toaster.Error("Please Select Location");
                return;
            }

            $.each(value.columns, function (k, v) {
                switch (v.toLowerCase()) {
                    case 'supplier':
                    case 'suppliername':
                    case 'supplier name':
                        csupplier = k;
                        break;
                    case 'product':
                        cproduct = k;
                        break;
                    case 'price':
                        cprice = k;
                        break;
                    case 'design':
                        cdesign = k;
                        break;
                    case 'brand':
                        cbrand = k;
                        break;
                    case 'size':
                        csize = k;
                        break;
                    case 'pricerange':
                    case 'price range':
                        cpricerange = k;
                        break;
                    case 'pattern':
                        cpattern = k;
                        break;
                    case 'style':
                        cstyle = k;
                        break;
                    case 'type':
                        ctype = k;
                        break;
                    case 'colour':
                        ccolour = k;
                        break;
                    case 'material':
                        cmaterial = k;
                        break;
                    case 'sleeve':
                        csleeve = k;
                        break;
                    case 'fit':
                        cfit = k;
                        break;
                    case 'stockrange':
                    case 'stock range':
                        cstockrange = k;
                        break;
                    case 'minorder':
                    case 'min order':
                        cminorder = k;
                        break;
                }
            });

            var missingrows = [];
            var skipped = 0;

            if (typeof ($scope.ReOrder.rules) == 'undefined' || $scope.ReOrder.rules == null)
                $scope.ReOrder.rules = [];

            $.each(value.rows, function (k, v) {
                if (k == 0) return;
                $scope.rule = {};
                $scope.rule.locationid = $scope.ReOrder.locationid;
                $scope.rule.productid = $scope.GetID(cproduct, v.values, $scope.AllProduct);
                $scope.rule.brandid = $scope.GetID(cbrand, v.values, $scope.AllBrand);
                $scope.rule.styleid = $scope.GetID(cstyle, v.values, $scope.AllStyle);
                $scope.rule.supplierid = $scope.GetID(csupplier, v.values, $scope.AllSupplier);
                $scope.rule.sizeid = $scope.GetID(csize, v.values, $scope.AllSizes);
                $scope.rule.patternid = $scope.GetID(cpattern, v.values, $scope.AllPattern);
                $scope.rule.colourid = $scope.GetID(ccolour, v.values, $scope.AllColors);
                $scope.rule.materialid = $scope.GetID(cmaterial, v.values, $scope.AllMaterial);
                $scope.rule.typeid = $scope.GetID(ctype, v.values, $scope.AllTypeList);
                $scope.rule.sleeveid = $scope.GetID(csleeve, v.values, $scope.AllSleeve);
                $scope.rule.fitid = $scope.GetID(cfit, v.values, $scope.AllFit);
                var range = $scope.GetID(cstockrange, v.values);
                if (range != null) {
                    $scope.rule.minro = range[0];
                    $scope.rule.maxro = range[1];
                }

                var minorder = $scope.GetID(cminorder, v.values);
                if (minorder && minorder != '')
                    $scope.rule.minorder = minorder;

                var price = $scope.GetID(cprice, v.values);
                if (price && price != '')
                    $scope.rule.price = price;

                var design = $scope.GetID(cdesign, v.values);
                if (design && design != '')
                    $scope.rule.designid = design;

                var range = $scope.GetID(cpricerange, v.values);
                if (range != null) {
                    $scope.rule.fromrate = range[0];
                    $scope.rule.torate = range[1];
                    $scope.rule.pricerange = range[0] + '-' + range[1];
                }
                $scope.rule.id = ($scope.ReOrder.rules.length + 1) * -1;
                var array = [];
                if ($scope.rule.supplierid > 0)
                    array.push('SUPLR:' + $scope.SearchRef($scope.rule.supplierid, $scope.Supplier));
                if ($scope.rule.sizeid > 0)
                    array.push('SIZE:' + $scope.SearchRef($scope.rule.sizeid, $scope.Sizes));
                if ($scope.rule.styleid > 0)
                    array.push('STYLE:' + $scope.SearchRef($scope.rule.styleid, $scope.Style));
                if ($scope.rule.brandid > 0)
                    array.push('BRAND:' + $scope.SearchRef($scope.rule.brandid, $scope.Brand));
                if ($scope.rule.patternid > 0)
                    array.push('PATRN:' + $scope.SearchRef($scope.rule.patternid, $scope.Pattern));
                if ($scope.rule.colourid > 0)
                    array.push('COLOUR:' + $scope.SearchRef($scope.rule.colourid, $scope.Colors));
                if ($scope.rule.materialid > 0)
                    array.push('MATRL:' + $scope.SearchRef($scope.rule.materialid, $scope.Material));
                if ($scope.rule.typeid > 0)
                    array.push('TYPE:' + $scope.SearchRef($scope.rule.typeid, $scope.TypeList));
                if ($scope.rule.fitid > 0)
                    array.push('FIT:' + $scope.SearchRef($scope.rule.fitid, $scope.AllFit));
                if ($scope.rule.sleeveid > 0)
                    array.push('SLEEVE:' + $scope.SearchRef($scope.rule.sleeveid, $scope.AllSleeve));
                $scope.rule.description = array.length == 0 ? 'All ' + $scope.SearchRef($scope.rule.productid, $scope.AllProduct) : array.join(',');
                $scope.ReOrder.rules.push($scope.rule);
            });
            $scope.ReportConfigGrid(50, true);
            $scope.cgrid.UpdateData($scope.ReOrder.rules);
        }

        $scope.Save = function () {
            request = {
                "Item": $scope.ReOrder,
                "SecurityToken": "String content"
            }

            if ($scope.markeddeleted.length > 0) {
                request.Item.deleted = $scope.markeddeleted;
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReorderrulesService/SaveAll",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Re Order Saved Successfully.!");
                $scope.ReOrder.rules = result.rules;
                $scope.cgrid.UpdateData($scope.ReOrder.rules);
                $scope.markeddeleted = [];
            }).error(function (result) {
            });
        }

        if (!$scope.ShowReport) {
            $timeout(function () {
                $scope.ReportConfigGrid(50);
            }, 10);
        }
    }]);



