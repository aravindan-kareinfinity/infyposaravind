
var app = angular.module('InfinityLab');

app.controller('targetmanagementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', 'Excel', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, Excel, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.ShowSaveAll = false;
        $scope.Mode = 'Configuration';
        $scope.SearchRef = restService.SearchReference;
        $scope.refemployeelist = restService.Get('ReferencelistService/EMPLOYEELEDGER', true).then(function (results) {
            $scope.refemployeelist = results;
            $scope.EmployeeBasedIncentive = $rootScope.UserInfo.appsettings.incentivemode == 6;
        });
        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

        restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.LocationList = (JSON.parse(JSON.stringify(results)));
            results.push({ 'id': -9, 'name': 'Location Base Target', 'code': 'Location Base Target' });
            results.push({ 'id': -10, 'name': 'Section Base Target', 'code': 'Section Base Target' });
            $scope.Locations = results;
        });

        $scope.Sources = [
            { 'id': 1, 'name': 'Bill' },
            { 'id': 2, 'name': 'Barcode' }];

        $scope.IncentiveModes = [
            { 'id': 0, 'name': 'No Age' },
            { 'id': 1, 'name': 'Age Based' },
            { 'id': 2, 'name': 'Date Based' }
        ];


        $scope.MonthList = [{ 'id': 1, 'name': 'JANUARY' },
        { 'id': 2, 'name': 'FEBRUARY' },
        { 'id': 3, 'name': 'MARCH' }, { 'id': 4, 'name': 'APRIL' },
        { 'id': 5, 'name': 'MAY' }, { 'id': 6, 'name': 'JUNE' },
        { 'id': 7, 'name': 'JULY' }, { 'id': 8, 'name': 'AUGUST' },
        { 'id': 9, 'name': 'SEPTEMBER' }, { 'id': 10, 'name': 'OCTOBER' },
        { 'id': 11, 'name': 'NOVEMBER' }, { 'id': 12, 'name': 'DECEMBER' }];

        $scope.Entity = {
            "id": 0,
            "employeeid": "",
            "fromdate": "",
            "iscurrent": 0,
            "basicpay": 0.0,
            "eligibleesi": false,
            "eligiblepf": false,
            "configuration": [],
            "pfselfpaid": 0,
            "esiselfpaid": 0.0,
            "grosspay": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "salarystructureid": 0,
            "createdon": new Date(),
            "modifiedon": new Date()
        }

        $scope.exporttoExcel = function (infygrid, title) {
            //var title = $rootScope.UserInfo.CompanyName;
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, '');
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


        $scope.SalaryManage = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SearchCondition = {
            sectionid: "",
            employeeid: "",
            departmentid: "",

        }

        $scope.SaveAll = function () {
            $scope.igrid.dataView.syncGridSelection($scope.igrid.grid, true);
            var grps = $scope.igrid.GetSelectedRows();
            var items = [];
            $.each(grps, function (k, v) {
                $.each(v.groups, function (kk, vv) {
                    items.push(vv);
                })
            })
            var request = {
                "Items": items,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeetargetService/SaveAll",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Target saved Successfully.!");
            }).error(function (result) {

            });
        }

        $scope.igrid = null;
        $scope.LastResult = [];
        $scope.ShowSectionResult = function (result) {

            $scope.LastResult = result;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 1);
            var newresult = [];
            var hasold = false;
            $.each(result, function (k, v) {
                if (v.otarget > 0 && v.otarget != v.target)
                    hasold = true;
                var targetgroup = newresult.find(function (item) {
                    return item.sectionid == v.sectionid && item.locationid == v.locationid;
                });
                if (targetgroup == undefined || targetgroup == null) {
                    targetgroup = (JSON.parse(JSON.stringify(v)));
                    targetgroup.sno = newresult.length + 1;
                    targetgroup.groups = [];
                    newresult.push(targetgroup);
                }
                targetgroup.groups.push(v);
            });

            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.Divisions);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.LocationList);

            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Detail',
                    name: 'Detail',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            var fromdate = new Date($scope.SearchCondition.targetfrom);
            var todate = new Date($scope.SearchCondition.targetto);
            while (fromdate < todate) {
                igrid.columns = [];
                var suffix = '' + fromdate.getFullYear() + '-' + (fromdate.getMonth() + 1);
                var column = igrid.AddCustomColumn('sales-' + suffix,
                    'Sales', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.sales, 2);
                        return '';
                    });
                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();

                if (hasold) {
                    var column = igrid.AddCustomColumn(
                        'otarget-' + suffix,
                        'Current', 80, function (item, col) {
                            var value = item.groups.find(function (item) {
                                return item.month == col.month &&
                                    item.year == col.year;
                            });
                            if (value && value != null)
                                return $filter('IndianNumber')(value.otarget, 2);
                            return '';
                        });
                    column.month = fromdate.getMonth() + 1;
                    column.year = fromdate.getFullYear();
                    column.total = true;
                    column.cssClass = 'rightalign';
                    column.getValue = function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return value.otarget;
                        return 0;
                    }
                }

                var column = igrid.AddCustomColumn(
                    'target-' + suffix,
                    hasold ? 'New' : 'Monthly', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.target, 2);
                        return '';
                    });

                column.getValue = function (item, col) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null)
                        return value.target;
                    return 0;
                }


                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();
                column.total = true;
                column.cssClass = 'rightalign';
                column.suffix = suffix;
                column.editor = Slick.Editors.Text;

                var column = igrid.AddCustomColumn(
                    'daily-' + suffix,
                    'Daily', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.dailytarget, 2);
                        return '';
                    });

                column.getValue = function (item, col) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null)
                        return value.dailytarget;
                    return 0;
                }

                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();
                column.total = true;
                column.cssClass = 'rightalign';
                column.editor = Slick.Editors.Text;
                column.suffix = suffix;

                groupColumns.push(
                    {
                        id: 'group-' + fromdate.getFullYear() +
                            '-' + fromdate.getMonth() + 1,
                        name: $scope.MonthList[fromdate.getMonth()].name + '-' + fromdate.getFullYear(),
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
                fromdate = fromdate.addMonths(1);
            }

            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();

            $scope.igrid = igrid;

            igrid.grid.onCellChange.subscribe(function (e, args) {

                var item = args.grid.getDataItem(args.row);
                var col = args.grid.getColumns()[args.cell];
                var colmn = col.id;
                if (colmn.startsWith('target-')) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null) {
                        value.target = item[colmn];
                    }
                    else {
                        value = {
                            'year': item.year,
                            'month': item.month,
                            'sectionid': item.sectionid,
                            'employeeid': item.employeeid,
                            'locationid': item.locationid,
                            'isactive': true,
                            'version': 1,
                            'target': item[colmn]
                        };
                        item.groups.push(value)
                    }
                    var target = parseFloat(value.target);
                    var monthdays = parseInt($scope.SearchCondition.monthdays);
                    if (!isNaN(target) && !isNaN(monthdays)) {
                        value.dailytarget = target / monthdays;
                        item['daily-' + col.suffix] = value.dailytarget;
                    }
                }
                if (colmn.startsWith('daily-')) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null) {
                        value.dailytarget = item[colmn];
                    }
                    else {
                        item.groups.push({
                            'year': item.year,
                            'month': item.month,
                            'sectionid': item.sectionid,
                            'employeeid': item.employeeid,
                            'locationid': item.locationid,
                            'isactive': true,
                            'version': 1,
                            'dailytarget': item[colmn]
                        })
                    }
                }

                $timeout(function () {
                    args.grid.invalidateRow(args.row);
                    args.grid.render();
                }, 10);

            });

            $scope.igrid.UpdateData(newresult, '_id');
        }


        $scope.ShowLocationResult = function (result) {

            $scope.LastResult = result;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 1);
            var newresult = [];
            var hasold = false;
            $.each(result, function (k, v) {
                if (v.otarget > 0 && v.otarget != v.target)
                    hasold = true;
                var targetgroup = newresult.find(function (item) {
                    return item.locationid == v.locationid;
                });
                if (targetgroup == undefined || targetgroup == null) {
                    targetgroup = (JSON.parse(JSON.stringify(v)));
                    targetgroup.sno = newresult.length + 1;
                    targetgroup.groups = [];
                    newresult.push(targetgroup);
                }
                targetgroup.groups.push(v);
            });

            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.LocationList);

            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Detail',
                    name: 'Detail',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            var fromdate = new Date($scope.SearchCondition.targetfrom);
            var todate = new Date($scope.SearchCondition.targetto);
            while (fromdate < todate) {
                igrid.columns = [];
                var suffix = '' + fromdate.getFullYear() + '-' + (fromdate.getMonth() + 1);
                var column = igrid.AddCustomColumn('sales-' + suffix,
                    'Sales', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.sales, 2);
                        return '';
                    });
                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();

                if (hasold) {
                    var column = igrid.AddCustomColumn(
                        'otarget-' + suffix,
                        'Current', 80, function (item, col) {
                            var value = item.groups.find(function (item) {
                                return item.month == col.month &&
                                    item.year == col.year;
                            });
                            if (value && value != null)
                                return $filter('IndianNumber')(value.otarget, 2);
                            return '';
                        });
                    column.month = fromdate.getMonth() + 1;
                    column.year = fromdate.getFullYear();
                    column.total = true;
                    column.cssClass = 'rightalign';
                    column.getValue = function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return value.otarget;
                        return 0;
                    }
                }

                var column = igrid.AddCustomColumn(
                    'target-' + suffix,
                    hasold ? 'New' : 'Monthly', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.target, 2);
                        return '';
                    });

                column.getValue = function (item, col) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null)
                        return value.target;
                    return 0;
                }


                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();
                column.total = true;
                column.cssClass = 'rightalign';
                column.suffix = suffix;
                column.editor = Slick.Editors.Text;

                var column = igrid.AddCustomColumn(
                    'daily-' + suffix,
                    'Daily', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.dailytarget, 2);
                        return '';
                    });

                column.getValue = function (item, col) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null)
                        return value.dailytarget;
                    return 0;
                }

                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();
                column.total = true;
                column.cssClass = 'rightalign';
                column.editor = Slick.Editors.Text;
                column.suffix = suffix;

                groupColumns.push(
                    {
                        id: 'group-' + fromdate.getFullYear() +
                            '-' + fromdate.getMonth() + 1,
                        name: $scope.MonthList[fromdate.getMonth()].name + '-' + fromdate.getFullYear(),
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
                fromdate = fromdate.addMonths(1);
            }

            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();

            $scope.igrid = igrid;

            igrid.grid.onCellChange.subscribe(function (e, args) {

                var item = args.grid.getDataItem(args.row);
                var col = args.grid.getColumns()[args.cell];
                var colmn = col.id;
                if (colmn.startsWith('target-')) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null) {
                        value.target = item[colmn];
                    }
                    else {
                        value = {
                            'year': item.year,
                            'month': item.month,
                            'sectionid': item.sectionid,
                            'employeeid': item.employeeid,
                            'locationid': item.locationid,
                            'isactive': true,
                            'version': 1,
                            'target': item[colmn]
                        };
                        item.groups.push(value)
                    }
                    var target = parseFloat(value.target);
                    var monthdays = parseInt($scope.SearchCondition.monthdays);
                    if (!isNaN(target) && !isNaN(monthdays)) {
                        value.dailytarget = target / monthdays;
                        item['daily-' + col.suffix] = value.dailytarget;
                    }
                }
                if (colmn.startsWith('daily-')) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null) {
                        value.dailytarget = item[colmn];
                    }
                    else {
                        item.groups.push({
                            'year': item.year,
                            'month': item.month,
                            'sectionid': item.sectionid,
                            'employeeid': item.employeeid,
                            'locationid': item.locationid,
                            'isactive': true,
                            'version': 1,
                            'dailytarget': item[colmn]
                        })
                    }
                }

                $timeout(function () {
                    args.grid.invalidateRow(args.row);
                    args.grid.render();
                }, 10);

            });

            $scope.igrid.UpdateData(newresult, '_id');
        }

        $scope.ShowResult = function (result) {

            $scope.LastResult = result;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 1);

            var newresult = [];
            var hasold = false;
            $.each(result, function (k, v) {
                if (v.otarget > 0 && v.otarget != v.target)
                    hasold = true;
                var targetgroup = newresult.find(function (item) {
                    return item.employeeid == v.employeeid;
                });
                if (targetgroup == undefined || targetgroup == null) {
                    targetgroup = (JSON.parse(JSON.stringify(v)));
                    targetgroup.sno = newresult.length + 1;
                    targetgroup.groups = [];
                    newresult.push(targetgroup);
                }
                targetgroup.groups.push(v);
            });

            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.Divisions);

            if ($scope.SearchCondition.locationid == -9) {
                igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.LocationList);
            }
            else {
                igrid.AddSimpleColumn('employeecode', 'Code', 60);
                igrid.AddSimpleColumn('name', 'Name', 100);
                igrid.AddFormatedColumn('salary', 'Salary', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('ratio', '%', 40, 'N0', 0);
            }

            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Detail',
                    name: 'Detail',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            var fromdate = new Date($scope.SearchCondition.targetfrom);
            var todate = new Date($scope.SearchCondition.targetto);
            while (fromdate < todate) {
                igrid.columns = [];
                var suffix = '' + fromdate.getFullYear() + '-' + (fromdate.getMonth() + 1);
                var column = igrid.AddCustomColumn('sales-' + suffix,
                    'Sales', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.sales, 2);
                        return '';
                    });
                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();

                if (hasold) {
                    var column = igrid.AddCustomColumn(
                        'otarget-' + suffix,
                        'Current', 80, function (item, col) {
                            var value = item.groups.find(function (item) {
                                return item.month == col.month &&
                                    item.year == col.year;
                            });
                            if (value && value != null)
                                return $filter('IndianNumber')(value.otarget, 2);
                            return '';
                        });
                    column.month = fromdate.getMonth() + 1;
                    column.year = fromdate.getFullYear();
                    column.total = true;
                    column.cssClass = 'rightalign';
                    column.getValue = function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return value.otarget;
                        return 0;
                    }
                }

                var column = igrid.AddCustomColumn(
                    'target-' + suffix,
                    hasold ? 'New' : 'Monthly', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.target, 2);
                        return '';
                    });

                column.getValue = function (item, col) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null)
                        return value.target;
                    return 0;
                }


                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();
                column.total = true;
                column.cssClass = 'rightalign';
                column.suffix = suffix;
                column.editor = Slick.Editors.Text;

                var column = igrid.AddCustomColumn(
                    'daily-' + suffix,
                    'Daily', 80, function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.month == col.month &&
                                item.year == col.year;
                        });
                        if (value && value != null)
                            return $filter('IndianNumber')(value.dailytarget, 2);
                        return '';
                    });

                column.getValue = function (item, col) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null)
                        return value.dailytarget;
                    return 0;
                }

                column.month = fromdate.getMonth() + 1;
                column.year = fromdate.getFullYear();
                column.total = true;
                column.cssClass = 'rightalign';
                column.editor = Slick.Editors.Text;
                column.suffix = suffix;

                groupColumns.push(
                    {
                        id: 'group-' + fromdate.getFullYear() +
                            '-' + fromdate.getMonth() + 1,
                        name: $scope.MonthList[fromdate.getMonth()].name + '-' + fromdate.getFullYear(),
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
                fromdate = fromdate.addMonths(1);
            }

            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();

            $scope.igrid = igrid;

            igrid.grid.onCellChange.subscribe(function (e, args) {

                var item = args.grid.getDataItem(args.row);
                var col = args.grid.getColumns()[args.cell];
                var colmn = col.id;
                if (colmn.startsWith('target-')) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null) {
                        value.target = item[colmn];
                    }
                    else {
                        value = {
                            'year': item.year,
                            'month': item.month,
                            'sectionid': item.sectionid,
                            'employeeid': item.employeeid,
                            'locationid': item.locationid,
                            'isactive': true,
                            'version': 1,
                            'target': item[colmn]
                        };
                        item.groups.push(value)
                    }
                    var target = parseFloat(value.target);
                    var monthdays = parseInt($scope.SearchCondition.monthdays);
                    if (!isNaN(target) && !isNaN(monthdays)) {
                        value.dailytarget = target / monthdays;
                        item['daily-' + col.suffix] = value.dailytarget;
                    }
                }
                if (colmn.startsWith('daily-')) {
                    var value = item.groups.find(function (item) {
                        return item.month == col.month &&
                            item.year == col.year;
                    });
                    if (value && value != null) {
                        value.dailytarget = item[colmn];
                    }
                    else {
                        item.groups.push({
                            'year': item.year,
                            'month': item.month,
                            'sectionid': item.sectionid,
                            'employeeid': item.employeeid,
                            'locationid': item.locationid,
                            'isactive': true,
                            'version': 1,
                            'dailytarget': item[colmn]
                        })
                    }
                }

                $timeout(function () {
                    args.grid.invalidateRow(args.row);
                    args.grid.render();
                }, 10);

            });

            $scope.igrid.UpdateData(newresult, '_id');
        }


        $scope.ShowIncentive = function (result) {

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 1);

            $.each(result, function (k, v) {
                v.sno = k + 1;
            });
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            if ($scope.SearchCondition.locationid == -9) {
                igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.LocationList);
                igrid.AddFormatedColumn('target', 'Target', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('sales', 'Sales', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('acheivedtarget', 'Acheived', 70, 'N0', 0).total = true;
            }
            else if ($scope.SearchCondition.locationid == -10) {
                igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.Divisions);
                igrid.AddFormatedColumn('target', 'Target', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('sales', 'Sales', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('acheivedtarget', 'Acheived', 70, 'N0', 0);
                var groupColumns = [];
                groupColumns.push(
                    {
                        id: 'group-detail',
                        name: 'Detail',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

                
                var grp = [];
                $.each(result, function (k, v) {
                    var egrp = grp.find(function (item) {
                        return item.sectionid == v.sectionid;
                    });
                    if (egrp == undefined || egrp == null) {
                        egrp = (JSON.parse(JSON.stringify(v)));
                        egrp.groups = [];
                        grp.push(egrp);
                    }
                    egrp.groups.push(v);
                })
                result = grp;
                $.each($scope.LocationList, function (k, v) {
                    if (v.id < 0) return;
                    igrid.columns = [];
                    var column = igrid.AddCustomColumn('target-' + v.id,
                        'Target', 80, function (item, col) {
                            var value = item.groups.find(function (item) {
                                return item.locationid == col.locationid;
                            });
                            if (value && value != null)
                                return $filter('IndianNumber')(value.target, 2);
                            return '';
                    });
                    column.getValue = function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.locationid == col.locationid;
                        });
                        if (value && value != null)
                            return value.target;
                        return 0;
                    }
                    column.locationid = v.id;
                    column.total = true;
                    var column = igrid.AddCustomColumn('sales-' + v.id,
                        'Sales', 80, function (item, col) {
                            var value = item.groups.find(function (item) {
                                return item.locationid == col.locationid;
                            });
                            if (value && value != null)
                                return $filter('IndianNumber')(value.sales, 2);
                            return '';
                    });
                    column.getValue = function (item, col) {
                        var value = item.groups.find(function (item) {
                            return item.locationid == col.locationid;
                        });
                        if (value && value != null)
                            return value.sales;
                        return 0;
                    }
                    column.locationid = v.id;
                    column.total = true;
                    var column = igrid.AddCustomColumn('achieve-' + v.id,
                        'Acheived', 80, function (item, col) {
                            var value = item.groups.find(function (item) {
                                return item.locationid == col.locationid;
                            });
                            if (value && value != null)
                                return $filter('IndianNumber')(value.acheivedtarget, 2);
                            return '';
                        });
                    column.locationid = v.id;
                    groupColumns.push(
                        {
                            id: 'location-' + v.id,
                            name: v.name,
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                })
                igrid.columns = groupColumns;
            }
            else {
                igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.Divisions);
                igrid.AddSimpleColumn('employeecode', 'Code', 60);
                igrid.AddSimpleColumn('name', 'Name', 100);
                igrid.AddFormatedColumn('salary', 'Salary', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('target', 'Target', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('sales', 'Total', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('oldsales', 'OLD', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('freshsales', 'NEW', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('acheivedtarget', 'Acheived', 70, 'N0', 0).total = true;
                igrid.AddFormatedColumn('incentive', 'Incentive', 70, 'N0', 0).total = true;
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result, '_id');
        }

        $scope.Search = function () {
            if ($scope.SearchCondition == undefined)
                $scope.SearchCondition = {};
            $scope.SearchCondition.datebased = true;
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            request.Item.showconfig = true;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeetargetService/SearchAll",
                data: srequest
            }).success(function (result) {
                var newresults = [];
                $.each(result, function (k, v) {
                    var employee = $scope.refemployeelist.find(function (item) {
                        return item.id == v.employeeid;
                    });
                    if (employee && employee != null) {
                        v.name = employee.name;
                        v.employeecode = employee.employeecode;
                        newresults.push(v);
                    }
                });
                if ($scope.SearchCondition.locationid == -9) {
                    $scope.ShowLocationResult(newresults);
                }
                else if ($scope.SearchCondition.locationid == -10) {
                    $scope.ShowSectionResult(newresults);
                }
                else {
                    $scope.ShowResult(newresults);
                }
                $scope.ShowSaveAll = false;
            }).error(function (result) {
            });
        }


        $scope.Incentive = function () {
            if ($scope.SearchCondition == undefined)
                $scope.SearchCondition = {};
            $scope.SearchCondition.datebased = true;
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            request.Item.showconfig = true;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeetargetService/Incentive",
                data: srequest
            }).success(function (result) {
                $scope.ShowIncentive(result)
            }).error(function (result) {
            });
        }

        $scope.ShowManual = function () {

            var sectionid = $scope.SearchCondition.sectionid;
            var fromdate = new Date($scope.SearchCondition.targetfrom);
            var todate = new Date($scope.SearchCondition.targetto);
            var list = [];
            while (fromdate < todate) {
                var month = fromdate.getMonth() + 1;
                var year = fromdate.getFullYear();
                var value = 0;
                if ($scope.LastResult && $scope.LastResult != null &&
                    $scope.LastResult.length > 0) {
                    var exist = $scope.LastResult.find(function (item) {
                        return item.sectionid == sectionid &&
                            item.month == month &&
                            item.year == year &&
                            item.sales > 0;
                    });
                    if (exist && exist != null)
                        value = exist.sales;
                }
                list.push({
                    'month': fromdate.getMonth() + 1,
                    'sectionid': sectionid,
                    'year': fromdate.getFullYear(),
                    'name': $scope.MonthList[fromdate.getMonth()].name,
                    'change': false,
                    'value': value
                });
                fromdate = fromdate.addMonths(1);
            }

            var dialog = ngDialog.open({
                template: 'ManualTarget',
                data: [$scope, list],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.MonthlyTarget = $scope.ngDialogData[1];
                    $scope.Change = function () {
                        $scope.closeThisDialog({
                            'MonthlyTarget': $scope.MonthlyTarget,
                            'SearchCondition': $scope.SearchCondition
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        if ($scope.LastResult && $scope.LastResult != null &&
                            $scope.LastResult.length > 0) {
                            var searchcondition = data.value.SearchCondition;
                            var targetpercentage = parseFloat(searchcondition.targetpercentage);
                            var monthdays = parseFloat(searchcondition.monthdays);
                            $.each(data.value.MonthlyTarget, function (k, v) {
                                var salesvalue = parseFloat(v.value);
                                $.each($scope.LastResult, function (kk, vv) {
                                    if (vv.year == v.year &&
                                        vv.month == v.month &&
                                        vv.sectionid == v.sectionid) {
                                        vv.sales = salesvalue;
                                        vv.attribs = {
                                            'incentivemode': searchcondition.incentivemode,
                                            'inc_freshpercentage': searchcondition.inc_freshpercentage,
                                            'inc_oldpercentage': searchcondition.inc_oldpercentage,
                                            'inc_stdpercentage': searchcondition.inc_stdpercentage,
                                            'cnf_freshdaypercentage': searchcondition.cnf_freshdaypercentage,
                                            'cnf_monthdays': searchcondition.cnf_monthdays,
                                            'eligibleprecentage': searchcondition.eligibleprecentage,
                                            'cnf_freshday': searchcondition.cnf_freshday,
                                            'cnf_freshdate': searchcondition.cnf_freshdate,
                                        };

                                        vv.target = Math.round(salesvalue * vv.ratio / 100);
                                        if (targetpercentage > 0) {
                                            vv.target += Math.round(vv.target * targetpercentage / 100)
                                        }
                                        if (monthdays > 0)
                                            vv.dailytarget = Math.round(vv.target / monthdays);
                                    }
                                });
                            });
                            $scope.ShowResult($scope.LastResult);
                        }
                    }
                }
            });
        }

        $scope.Generate = function () {
            $scope.EnableManual = false;
            $scope.SearchCondition.billsource = $scope.SearchCondition.sourceid;

            if ($scope.SearchCondition.locationid == -9) {
                $scope.SearchCondition.locationlist = [];
                $.each($scope.LocationList, function (k, v) {
                    if (v.selected) {
                        $scope.SearchCondition.locationlist.push(v.id);
                    }
                })
            }

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            request.Item.showconfig = true;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeetargetService/Build",
                data: srequest
            }).success(function (result) {

                if ($scope.SearchCondition.locationid == -9) {
                    $scope.ShowLocationResult(result);
                }
                else if ($scope.SearchCondition.locationid == -10) {
                    $scope.ShowSectionResult(result);
                }
                else {
                    $scope.ShowResult(result);
                }

                $scope.EnableManual = $scope.SearchCondition.sectionid > 0;
            }).error(function (result) {
            });
        }

    }]);

