
var app = angular.module('InfinityLab');

app.controller('EmployeeLeaveController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReferencebyCode;

        $scope.WorkingModes = [{ 'id': -1, 'name': 'All' }, { 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': -1, 'name': 'All' }, { 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];

        $scope.workingdays = 0;
        $scope.workingmode = -1;
        $scope.salarymode = -1;

        $scope.companyname = $rootScope.UserInfo.CompanyName;


        $scope.refstatuslist = restService.Get('ReferencelistService/LEAVESTATUS', true).then(function (results) {
            $scope.refstatuslist = results;
        });

        $scope.mode = '1';
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.refleavetypelist = restService.Get('ReferencelistService/LEAVETYPE', true).then(function (results) {
            $scope.refleavetypelist = results;
        });

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Division = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Division = results;
        });

        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });

        $scope.LimitCheck = 50;

        //$scope.ScrollCheckList = function () {
        //    if ($scope.LimitCheck < $scope.CheckResult.employees.length) {
        //        $scope.LimitCheck = $scope.LimitCheck + 20;
        //        if ($scope.LimitCheck > 0 && $scope.CheckResult.employees.length)
        //            $scope.LimitCheck = $scope.CheckResult.employees.length;
        //    }
        //};


        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
        { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
        { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
        { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
        { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
        { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];

        $scope.year = new Date().getFullYear();

        $scope.refTimeMode = [
            { id: 0, name: "Full" },
            { id: 1, name: "Morning" },
            { id: 2, name: "Evening" }
        ];

        $scope.Modes = [
            { id: 0, name: "Manual" },
            { id: 1, name: "Auto" },
            { id: 2, name: "All" },
            { id: 3, name: "Summary" },
            { id: 4, name: "Check" }
        ];


        $scope.Entity = {

            "employeeid": 0,
            "leavetype": null,
            "reason": null,
            "fromdate": "",
            "todate": "",
            "fromdatehalfday": 0,
            "todatehalfday": 0,
            "days": 0,
            "ishalfday": "",
            "issunday": "",
            "deductsalary": false,
            "status": "REQUEST",
            "approvedby ": "",
            "institutionid": $scope.logininstitutionid,
            "version": 1,
            "isactive": true,
            "modifiedon": new Date(),
            "createdon": new Date(),
            "createdby": 0,
            "modifiedby": 0,
        };
        $scope.ApplyLeave = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.SearchCondition = {
            employeeid: "",
            month: "",
            year: "",
            companyid: "",
            locationid: "",
            departmentid: ""
        }
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.Status = false;
        $scope.ShowNew = false;
        $scope.SwitchPanel = function () {
            $scope.ShowNew = !$scope.ShowNew;
        }
        $scope.AddNew = function () {
            $scope.ApplyLeave = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ApplyLeave.status = "REQUEST"
            $scope.ShowNew = true;
        }
        $scope.CheckLeave = function () {
            var exists = $scope.refleavetypelist.find(function (item) {
                return item.code == $scope.ApplyLeave.leavetype;
            });

            if (exists != null) {
                $scope.ApplyLeave.deductsalary = exists.attribute.deductsalary;
            }
        }
        $scope.Save = function () {
            if ($scope.ApplyLeave.employeeid == null || $scope.ApplyLeave.employeeid == 0) {
                $rootScope.Toaster.Error("Please select Employee");
                return;
            }

            if (typeof ($scope.ApplyLeave.approved) != 'undefined' && $scope.ApplyLeave.approved) {
                if ($scope.ApplyLeave.approvedby == null || $scope.ApplyLeave.approvedby == 0) {
                    $rootScope.Toaster.Error("Please select the approver");
                    return;
                }
            }
            else {
                $scope.ApplyLeave.approvedby = 0;
            }

            
            request = {
                "Item": $scope.ApplyLeave,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeleaveService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.ApplyLeave = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.Status = false;
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }

        $scope.leavegrid = null;
        $scope.ConfigLeave = function () {
            if ($scope.leavegrid != null)
                $scope.leavegrid.destroy();

            var igrid = new InfyGrid('gridLeave', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddSimpleColumn('employeename', 'Name', 150);
            igrid.AddSimpleColumn('designation', 'Designation', 100);
            igrid.AddFormatedColumn('fromdate', 'From', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('todate', 'To', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('days', 'Days', 75);
            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;";
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;";

            igrid.AddButtonColumn('action', 'Action', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                });
                return true;
            });

            igrid.Initialize();
            $scope.leavegrid = igrid;
            $scope.leavegrid.UpdateData($scope.SearchResult);
        }

        $scope.exportigrid = function (infygrid, filename, subtitle) {
            var title = $rootScope.UserInfo.CompanyName;
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

        $scope.exporttoExcel = function (tableid, title) {

            var removecol = 1;

            if ($scope.CheckResult != null) {
                tableid = 'leavecheck';
                removecol = 0;
            }
            else if ($scope.SummaryResult.length > 0) {
                tableid = 'leavesummary';
                removecol = 0;
            }

            $scope.exportHref = Excel.tableToExcel(tableid, title, removecol, 'LEAVE STAFF LIST -' + $filter('date')($scope.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.todate, 'dd-MMM-yyyy'));
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.Delete = function (item) {
            var request = {
                "Item": item,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EmployeeleaveService/Delete",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.ErrorMessage = "Successfully deleted";
                    $scope.ApplyLeave = (JSON.parse(JSON.stringify($scope.Entity)));
                }

            }).error(function (result) {

            });
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
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
                                    url: "EmployeeleaveService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
	jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
            }
            else if (action == "Edit") {
                $scope.ShowNew = true;
                $scope.ApplyLeave = item;
                $scope.ApplyLeave.approved = $scope.ApplyLeave.approvedby > 0;
            }
        }

        $scope.alldaymenuOptions = [
            ['PRESENT ALL DAYS', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'P', false, true);
            }],
            ['ABSENT ALL DAYS', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'A', false, true);
            }],
            ['1H ALL DAYS', function ($itemScope) {
                $scope.DirectLeave($itemScope, '1H', false, true);
            }],
            ['2H ALL DAYS', function ($itemScope) {
                $scope.DirectLeave($itemScope, '2H', false, true);
            }]
        ];

        $scope.allmenuOptions = [
            ['PRESENT ALL EMPLOYEES', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'P', true, false);
            }],
            ['ABSENT ALL EMPLOYEES', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'A', true, false);
            }],
            ['1H ALL EMPLOYEES', function ($itemScope) {
                $scope.DirectLeave($itemScope, '1H', true, false);
            }],
            ['2H ALL EMPLOYEES', function ($itemScope) {
                $scope.DirectLeave($itemScope, '2H', true, false);
            }]
        ];

        $scope.menuOptions = [
            ['PRESENT', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'P', false, false);
            }],
            ['ABSENT', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'A', false, false);
            }],
            ['1H', function ($itemScope) {
                $scope.DirectLeave($itemScope, '1H', false, false);
            }],
            ['2H', function ($itemScope) {
                $scope.DirectLeave($itemScope, '2H', false, false);
            }],
            ['1HL', function ($itemScope) {
                $scope.DirectLeave($itemScope, '1HL', false, false);
            }],
            ['2HL', function ($itemScope) {
                $scope.DirectLeave($itemScope, '2HL', false, false);
            }],
            ['L', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'L', false, false);
            }],
            ['OD', function ($itemScope) {
                $scope.DirectLeave($itemScope, 'OD', false, false);
            }]
        ];

        var renderContextMenu = function ($scope, event, options) {
            if (!$) { var $ = angular.element; }
            $(event.currentTarget).addClass('context');
            var $contextMenu = $('<div>');
            $contextMenu.addClass('dropdown clearfix');
            var $ul = $('<ul>');
            $ul.addClass('dropdown-menu');
            $ul.attr({ 'role': 'menu' });
            $ul.css({
                display: 'block',
                position: 'absolute',
                left: event.pageX + 'px',
                top: event.pageY + 'px'
            });
            angular.forEach(options, function (item, i) {
                var $li = $('<li>');
                if (item === null) {
                    $li.addClass('divider');
                } else {
                    var $a = $('<a>');
                    $a.attr({ tabindex: '-1', href: '#' });
                    $a.text(typeof item[0] == 'string' ? item[0] : item[0].call($scope, $scope));
                    $li.append($a);
                    $li.on('click', function ($event) {
                        $event.preventDefault();
                        $scope.$apply(function () {
                            $(event.currentTarget).removeClass('context');
                            $contextMenu.remove();
                            item[1].call($scope, $scope);
                        });
                    });
                }
                $ul.append($li);
            });
            $contextMenu.append($ul);
            var height = Math.max(
                document.body.scrollHeight, document.documentElement.scrollHeight,
                document.body.offsetHeight, document.documentElement.offsetHeight,
                document.body.clientHeight, document.documentElement.clientHeight
            );
            $contextMenu.css({
                width: '100%',
                height: height + 'px',
                position: 'absolute',
                top: 0,
                left: 0,
                zIndex: 9999
            });
            $(document).find('body').append($contextMenu);
            $contextMenu.on("mousedown", function (e) {
                if ($(e.target).hasClass('dropdown')) {
                    $(event.currentTarget).removeClass('context');
                    $contextMenu.remove();
                }
            }).on('contextmenu', function (event) {
                $(event.currentTarget).removeClass('context');
                event.preventDefault();
                $contextMenu.remove();
            });
        };

        $scope.DirectLeave = function ($itemScope, status, allemp, allday) {
            var direct = {};
            var employeeids = [];
            if (allemp) {
                $.each($scope.CheckResult.employees, (function (k, v) {
                    if (v.status[$itemScope.CurrentCell.item.days] != '-')
                        employeeids.push(v.id);
                }));
                direct = {
                    'date': $itemScope.CurrentCell.item.date,
                    'employeeid': -1,
                    'employeeids': employeeids,
                    'oldstatus': 'E',
                    'status': status
                };
            }
            else if (allday) {
                var dates = [];
                $.each($scope.CheckResult.days, (function (k, v) {
                    if ($itemScope.CurrentCell.item.status[v.days] != '-')
                        dates.push(v.date);
                }));
                direct = {
                    'dates': dates,
                    'employeeid': $itemScope.CurrentCell.item.id,
                    'employeeids': -2,
                    'oldstatus': 'D',
                    'status': status
                };
            }
            else {
                direct = {
                    'date': $itemScope.CurrentCell.ditem.date,
                    'employeeid': $itemScope.CurrentCell.item.id,
                    'oldstatus': $itemScope.CurrentCell.item.status[$itemScope.CurrentCell.ditem.days],
                    'status': status
                };
                if (direct.status == direct.oldstatus || direct.oldstatus == '-') return;
            }



            var request = {
                "Item": direct,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EmployeeleaveService/Direct",
                data: srequest
            }).success(function (result) {

                if (allemp) {
                    $.each($scope.CheckResult.employees, (function (k, v) {
                        if (v.status[$itemScope.CurrentCell.item.days] != '-')
                            v.status[$itemScope.CurrentCell.item.days] = direct.status;
                    }));
                }
                else if (allday) {
                    $.each($scope.CheckResult.days, (function (k, v) {
                        if ($itemScope.CurrentCell.item.status[v.days] != '-')
                            $itemScope.CurrentCell.item.status[v.days] = direct.status;
                    }));
                }
                else {
                    $itemScope.CurrentCell.item.status[$itemScope.CurrentCell.ditem.days] = direct.status;
                }

                $scope.igrid.UpdateData($scope.CheckResult.employees);

            }).error(function (result) {

            });
        }

        $scope.displayLeave = function (item, ditem) {
            return item.status[ditem.days];
        }

        $scope.ManageLeave = function (action, item) {
            item.oldstatus = item.status;
            item.status = action;

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EmployeeleaveService/Save",
                data: srequest
            }).success(function (result) {
                item.status = result.status;
                item.version = result.version;
            }).error(function (result) {
                item.status = item.oldstatus;
            });
        }

        $scope.SearchResult = [];
        $scope.SummaryResult = [];
        $scope.CheckResult = null;
        $scope.CheckPageResult = null;
        $scope.Status = "";
        $scope.LoadCheckList = function () {

            var max = $scope.CheckPageResult.employees.length > 50 ? 50 : $scope.CheckPageResult.employees.length;
            $scope.Status = "Loads remaining " + $scope.CheckPageResult.employees.length;
            for (var i = 0; i < max; i++) {
                $scope.CheckResult.employees.push($scope.CheckPageResult.employees.shift());
            };

            if ($scope.CheckPageResult.employees.length > 0) {
                $scope.Status = "Rendering remaining " + $scope.CheckPageResult.employees.length;
                $timeout(function () {
                    $scope.LoadCheckList();
                }, 10);
            }
            else {
                $scope.Status = "Completed..";
                $timeout(function () {
                    $scope.Status = "";
                }, 50);
            }

        };

        $scope.RenderCheckListGrid = function () {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridleavecheck').empty();
            }
            var igrid = new InfyGrid('gridleavecheck', '500px', '100%', true, false, true, 0);
            var groupColumns = [];
            igrid.AddSimpleColumn('code', 'Code', 50);
            igrid.AddSimpleColumn('name', 'Name', 150);
            igrid.AddSimpleColumn('designation', 'Designation', 100);

            igrid.AddCustomColumn('leave', 'Leave', 50, function (item, col) {
                return (item.leavedays);
            }).cssClass = 'rightalign';

            igrid.AddCustomColumn('absent', 'Absent', 50, function (item, col) {
                return (item.absent);
            }).cssClass = 'rightalign';;

            igrid.AddCustomColumn('days', 'Present', 50, function (item, col) {
                return (item.totaldays - item.absent - (item.totaldays - item.days));
            }).cssClass = 'rightalign';

            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            $.each($scope.CheckResult.days, function (k, v) {
                igrid.columns = [];
                var column = igrid.AddCustomColumn('GDay-' + k, $filter('date')(v.date, 'EEE'), 50, function (item, col) {
                    return item.status[col.internal.days]
                });
                column.internal = v;
                groupColumns.push(
                    {
                        id: 'Day-' + k,
                        name: $filter('date')(v.date, 'dd'),
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            });

            igrid.columns = groupColumns;

            igrid.Initialize();
            $scope.igrid = igrid;

            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if (item.absent != item.leavedays) {
                    item.style = 'igridwithgrey';
                    return { cssClasses: 'igridwithgrey' };
                }
                item.style = '';
                return { cssClasses: '' };
            };

            igrid.grid.setOptions({ 'frozenColumn': 5 });
            
            igrid.grid.onHeaderContextMenu.subscribe(function (e,args) {
                e.preventDefault();
                if (typeof (args.column.internal) != 'undefined' && args.column.internal) {
                    $scope.CurrentCell = {
                        'type': 'header',
                        'item': args.column.internal,
                        'days': args.column.internal.days,
                        'date': args.column.internal.date
                    };
                    renderContextMenu($scope, e, $scope.allmenuOptions);
                }
            });

            igrid.grid.onContextMenu.subscribe(function (e) {
                e.preventDefault();
                var cell = igrid.grid.getCellFromEvent(e);
                var row = cell.row;
                var column = cell.cell;
                var itemdata = igrid.grid.getDataItem(row);
                var options = null;
                if (column == 1 || column == 2) {
                    options = $scope.alldaymenuOptions;
                    $scope.CurrentCell = {
                        'type': 'employee', 'item': itemdata
                    };
                }
                else if (column >= 5) {
                    $scope.CurrentCell = {
                        'type': 'day', 'item': itemdata,
                        'ditem': igrid.grid.getColumns()[column].internal
                    };
                    options = $scope.menuOptions;
                }
                if (options != null)
                    renderContextMenu($scope, e, options);
            });

            if ($scope.syncinoutleaves) {
                $scope.igrid.UpdateData($scope.CheckResult.employees.filter(function (item) {
                    return item.leavedays != item.absent;
                }));
            }
            else {
                $scope.igrid.UpdateData($scope.CheckResult.employees);
            }
        }

        $scope.Search = function () {
            var request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    departmentid: $scope.departmentid,
                    locationid: $scope.locationid,
                    sectionid: $scope.sectionid,
                    floorid: $scope.floorid,
                    companyid: $scope.companyid,
                    searchmode: $scope.mode,
                    fromdate: $scope.fromdate,
                    todate: $scope.todate,
                    employeecode: $scope.employeecode,
                    salarymode: $scope.salarymode,
                    workdays_exceptholiday: $scope.workdays_exceptholiday,
                    syncinoutleaves: $scope.syncinoutleaves,
                    workingmode: $scope.workingmode
                },
                "SecurityToken": "String content"
            }

            $scope.Status = "Searching...";

            var timeDiff = (new Date($scope.todate)) - (new Date($scope.fromdate));
            $scope.workingdays = Math.round(timeDiff / (1000 * 60 * 60 * 24));

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeleaveService/Search",
                data: srequest
            }).success(function (result) {
                $scope.Status = "";
                if (request.Item.searchmode == 3) {
                    $scope.SummaryResult = result;
                    $scope.working = result.days.length;
                    $scope.SearchResult = [];
                    $scope.CheckResult = null;
                }
                else if (request.Item.searchmode == 4) {

                    $scope.CheckResult = result;
                    $timeout(function () {
                        $scope.RenderCheckListGrid();
                    }, 50);

                    //if (result.employees.length > 100) {
                    //    $scope.CheckPageResult = result;
                    //    $scope.CheckResult = { 'days': result.days, 'employees': [] };
                    //    $scope.Status = "Loading...";
                    //    $scope.LoadCheckList();
                    //}
                    //else {
                    //    $scope.CheckResult = result;
                    //}

                    $scope.SearchResult = [];
                    $scope.SummaryResult = [];
                    
                }
                else {
                    $scope.SearchResult = result;
                    $scope.SummaryResult = [];
                    $scope.CheckResult = null;
                    $timeout(function () {
                        $scope.ConfigLeave();
                    },50);
                }
            }).error(function (result) {
            });

        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);