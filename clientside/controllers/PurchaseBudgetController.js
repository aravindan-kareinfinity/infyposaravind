var app = angular.module('InfinityLab');

app.controller('PurchaseBudgetController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$stateParams', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $stateParams, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.ReportList = [{ 'id': 1, 'name': 'Budget Summary' },
        { 'id': 2, 'name': 'Manager Summary' },
        { 'id': 3, 'name': 'Product Summary' }];
        $scope.ReportPage = false;
        $scope.ProductLoaded = false;
        $scope.Budgets = [];
        restService.Get('ReferencelistService/FULL-POTYPE', true).then(function (results) {
            var Budgets = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo && v.additionalinfo.isonline)
                    Budgets.push(v);
            })
            $scope.Budgets = Budgets;
        });

        restService.Get('ReferencelistService/PURCHASEMANAGER', true).then(function (results) {
            $scope.PurchaseManager = results;
        });
        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });
        $scope.LocationList = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/LIST-PRODUCT', true).then(function (results) {
            $scope.ProductList = results;
            $scope.ProductLoaded = true;
        });

        $scope.Save = function () {
            var request = {
                "items": [],
                "securitytoken": "String content"
            }

            $.each($scope.TotalEmployees, function (k, v) {
                if (v.modified)
                    request.items.push(v);
            });

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchasebudgetService/SaveAll",
                data: srequest
            }).success(function (result) {
                $scope.Search();
            }).error(function (result) {
            });
        }


        $scope.Report = function () {

            $scope.SearchCondition.budgetids = [];
            $scope.SearchCondition.managerids = [];
            $scope.SearchCondition.productids = [];

            $.each($scope.sprdgrid.GetSelectedRows(), function (k, v) {
                $scope.SearchCondition.productids.push(v.id);
            })
            $.each($scope.smnggrid.GetSelectedRows(), function (k, v) {
                $scope.SearchCondition.managerids.push(v.id);
            })
            $.each($scope.sbdggrid.GetSelectedRows(), function (k, v) {
                $scope.SearchCondition.budgetids.push(v.id);
            })

            var request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchasebudgetService/Report",
                data: srequest
            }).success(function (result) {
                $scope.ShowReport(result);
            }).error(function (result) {
            });

        }

        $scope.HasConfig = false;
        $scope.ShowConfig = function () {
            $scope.ReportPage = !$scope.ReportPage;
            if (!$scope.ReportPage)
                return;
            if ($scope.HasConfig)
                return;

            $scope.HasConfig = true;
            $timeout(function () {
                var igrid = new InfyGrid('sprdgrid', '425px', '100%', false, true, true, 400);
                igrid.AddSimpleColumn('name', 'Product', 250);
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData($scope.ProductList);
                $scope.sprdgrid = igrid;

                var igrid = new InfyGrid('smnggrid', '425px', '100%', false, true, true, 400);
                igrid.AddSimpleColumn('name', 'Manager', 250);
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData($scope.PurchaseManager);
                $scope.smnggrid = igrid;

                var igrid = new InfyGrid('sbdggrid', '425px', '100%', false, true, true, 420);
                igrid.AddSimpleColumn('name', 'Budget', 200);
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData($scope.Budgets);
                $scope.sbdggrid = igrid;
            }, 20);
        }

        $scope.ShowReport = function (reportitems) {

            if ($scope.rgrid != null) {
                $scope.rgrid.grid.destroy();
                $('#rptContainer').empty();
            }


            var igrid = new InfyGrid('rptContainer', '425px', '100%', true, true, true, 0);
            if ($scope.SearchCondition.reportid == 1) {
                igrid.AddSimpleColumn('budgetname', 'Budget', 120);
                igrid.AddFormatedColumn('configbudget', 'Total', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('configbudgetproducts', 'Products', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('orderqty', 'Ordered Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('receivedqty', 'Received Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('ordervalue', 'Ordered Value', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('purchaseqty', 'Purchase Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('purchasevalue', 'Purchase Value', 80, 'N2', 2).total = true;
            }
            if ($scope.SearchCondition.reportid == 2) {
                igrid.AddSimpleColumn('budgetname', 'Budget', 120);
                igrid.AddSimpleColumn('employee', 'Manager', 120);
                igrid.AddFormatedColumn('configbudget', 'Total', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('configbudgetproducts', 'Products', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('orderqty', 'Ordered Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('receivedqty', 'Received Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('ordervalue', 'Ordered Value', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('purchaseqty', 'Purchase Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('purchasevalue', 'Purchase Value', 80, 'N2', 2).total = true;
            }
            if ($scope.SearchCondition.reportid == 3) {
                igrid.AddSimpleColumn('budgetname', 'Budget', 120);
                igrid.AddSimpleColumn('employee', 'Manager', 120);
                igrid.AddSimpleColumn('product', 'Product', 120);
                igrid.AddFormatedColumn('configbudget', 'Total Budget', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('configbudgetproducts', 'Product Budget', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('productbudgetconfig', 'Product Budget', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('orderqty', 'Ordered Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('receivedqty', 'Received Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('ordervalue', 'Ordered Value', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('purchaseqty', 'Purchase Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('purchasevalue', 'Purchase Value', 80, 'N2', 2).total = true;
            }

            igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.rgrid = igrid;
            $scope.rgrid.UpdateData(reportitems, '_id');
        }

        $scope.Search = function () {
            var request = {
                "item": { 'budgetid': $scope.ProductBudget.budgetid },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchasebudgetService/Search",
                data: srequest
            }).success(function (result) {

                var employees = [];
                $.each($scope.PurchaseManager, function (k, v) {
                    var exist = result.find(function (item) {
                        return item.employeeid == v.id;
                    });
                    var emp = {
                        'employeeid': v.id, 'name': v.name, 'isactive': true,
                        'budgetid': $scope.ProductBudget.budgetid, 'alloted': 0,
                        'budget': 0, 'budgets': [], 'version': 0
                    };
                    if (exist && exist != null) {
                        emp = exist;
                        emp.name = v.name;
                    }
                    employees.push(emp);
                })
                $scope.GridConfig(employees);
            }).error(function (result) {
            });

        }

        $scope.igrid = null;
        $scope.pgrid = null;
        $scope.CurrentEmployee = null;
        $scope.TotalEmployees = [];
        $scope.GridConfig = function (result) {
            $scope.TotalEmployees = result;
            $scope.ModifiedCount = 0;
            $scope.pgrid = null;
            $scope.CurrentEmployee = null;
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;

            igrid.AddButtonColumn('load', '', 40, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-primary btn-xs'><i class='fa fa-gear' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.AddSimpleColumn('name', 'Manager', 180);

            var column = igrid.AddCustomColumn('budget', "Budget", 80, function (item, col) {
                return item.budget;
            });
            column.cssClass = 'rightalign';
            column.sortable = true;
            column.total = true;
            column.filtertype = 'number';
            column.getValue = function (item, col) {
                return item.budget;
            };
            column.setValue = function (item, col, newvalue, callback) {
                var lastValue = col.getValue(item, col);
                var newvalue = parseFloat(newvalue);
                if (isNaN(newvalue))
                    newvalue = 0;
                if (lastValue != newvalue) {
                    item.budget = newvalue;
                    item.modified = true;
                }
            }
            column.editor = Slick.Editors.Text;
            var column = igrid.AddCustomColumn('alloted', 'Alloted', 80, function (item, col) {
                return $filter('IndianNumber')(item.alloted, 2);
            });
            column.cssClass = 'rightalign';
            column.cssClass = 'rightalign';
            column.sortable = true;
            column.total = true;
            column.filtertype = 'number';
            column.getValue = function (item, col) {
                return item.alloted;
            };

            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result, '_id');
            igrid.grid.onCellChange.subscribe(function (e, args) {
                var command = args.grid.getColumns()[args.cell].id;
                var item = args.grid.getDataItem(args.row);
                item.modified = true;
                $timeout(function () {
                    $scope.UpdateModified();
                }, 10);
            });

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'load') {
                    $timeout(function () {
                        item.rowindex = cell.row;
                        $scope.ShowProductBudget(item)
                        $scope.$digest();
                    }, 10);
                }
            });
        }

        $scope.AddProduct = function () {
            if ($scope.CurrentEmployee == null || $scope.pgrid == null) return;
            $scope.CurrentEmployee.budgets.push({ 'productid': $scope.ProductBudget.productid, 'value': '', 'percentage': '' });
            $scope.pgrid.UpdateData($scope.CurrentEmployee.budgets, '_id');
        }

        $scope.ModifiedCount = 0;
        $scope.UpdateModified = function () {
            $scope.ModifiedCount = 0;
            $.each($scope.TotalEmployees, function (k, v) {
                if (v.modified)
                    $scope.ModifiedCount++;
            })
        }

        $scope.ShowProductBudget = function (item) {

            $scope.CurrentEmployee = item;
            if ($scope.pgrid != null) {
                $scope.pgrid.grid.destroy();
                $('#prdContainer').empty();
            }

            var igrid = new InfyGrid('prdContainer', '425px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('productid', 'Products', 180, $scope.ProductList);

            var column = igrid.AddCustomColumn('value', "Value", 80, function (item, col) {
                return item.value;
            });
            column.cssClass = 'rightalign';
            column.sortable = true;
            column.total = true;
            column.filtertype = 'number';
            column.getValue = function (item, col) {
                return item.value;
            };
            column.setValue = function (item, col, newvalue, callback) {
                var lastValue = col.getValue(item, col);
                var newvalue = parseFloat(newvalue);
                if (isNaN(newvalue))
                    newvalue = 0;
                if (lastValue != newvalue) {
                    item.value = newvalue;
                    var value = 0;
                    $scope.CurrentEmployee.modified = true;
                    $.each($scope.CurrentEmployee.budgets, function (k, v) {
                        var nvalue = parseFloat(v.value);
                        if (isNaN(nvalue)) nvalue = 0;
                        value += nvalue;
                    })
                    $scope.CurrentEmployee.alloted = value;
                    $scope.igrid.grid.invalidateRow($scope.CurrentEmployee.rowindex);
                }
            }
            column.editor = Slick.Editors.Text;

            var column = igrid.AddCustomColumn('percentage', "%", 80, function (item, col) {
                return item.percentage;
            });
            column.cssClass = 'rightalign';
            column.sortable = true;
            column.total = true;
            column.filtertype = 'number';
            column.getValue = function (item, col) {
                return item.percentage;
            };
            column.setValue = function (item, col, newvalue, callback) {
                var lastValue = col.getValue(item, col);
                var newvalue = parseFloat(newvalue);
                if (isNaN(newvalue))
                    newvalue = 0;
                if (lastValue != newvalue) {
                    item.percentage = newvalue;
                }
            }

            column.editor = Slick.Editors.Text;
            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.pgrid = igrid;
            $scope.pgrid.UpdateData($scope.CurrentEmployee.budgets, '_id');

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var command = args.grid.getColumns()[args.cell].id;
                var item = args.grid.getDataItem(args.row);

                if (command == 'value') {

                    if ($scope.CurrentEmployee.budget &&
                        $scope.CurrentEmployee.budget > 0) {
                        var value = parseFloat(item.value);
                        if (isNaN(value)) value = 0;
                        item.percentage = (value / $scope.CurrentEmployee.budget * 100).round(2);
                    }

                    var value = 0;
                    $scope.CurrentEmployee.modified = true;
                    $.each($scope.CurrentEmployee.budgets, function (k, v) {
                        var nvalue = parseFloat(v.value);
                        if (isNaN(nvalue)) nvalue = 0;
                        value += nvalue;
                    })
                    $scope.CurrentEmployee.alloted = value;
                    $timeout(function () {
                        $scope.igrid.grid.invalidateRow($scope.CurrentEmployee.rowindex);
                        $scope.igrid.grid.render();
                        if ($scope.CurrentEmployee.budget &&
                            $scope.CurrentEmployee.budget > 0) {
                            $scope.pgrid.UpdateAllTotals(args.grid);
                            args.grid.invalidateRow(args.row);
                            args.grid.render();
                        }
                        $scope.UpdateModified();
                    }, 10);
                    return;
                }

                if (command == 'percentage') {
                    var value = 0;
                    $scope.CurrentEmployee.modified = true;
                    if ($scope.CurrentEmployee.budget &&
                        $scope.CurrentEmployee.budget > 0) {
                        var percentage = parseFloat(item.percentage);
                        if (isNaN(percentage)) percentage = 0;
                        item.value = ($scope.CurrentEmployee.budget * percentage / 100).round(0);
                    }
                    $.each($scope.CurrentEmployee.budgets, function (k, v) {
                        var nvalue = parseFloat(v.value);
                        if (isNaN(nvalue)) nvalue = 0;
                        value += nvalue;
                    })
                    $scope.CurrentEmployee.alloted = value;
                    $timeout(function () {
                        $scope.igrid.grid.invalidateRow($scope.CurrentEmployee.rowindex);
                        $scope.igrid.grid.render();
                        if ($scope.CurrentEmployee.budget &&
                            $scope.CurrentEmployee.budget > 0) {
                            $scope.pgrid.UpdateAllTotals(args.grid);
                            args.grid.invalidateRow(args.row);
                            args.grid.render();
                        }
                        $scope.UpdateModified();
                    }, 10);
                    return;
                }

            });
        }

    }]);