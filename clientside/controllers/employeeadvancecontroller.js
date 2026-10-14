
var app = angular.module('InfinityLab');

app.controller('EmployeeAdvanceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('SEARCH');
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        //$scope.companyname = 'PAKSHI K RAMANATHAN CHETTIYAR SON';
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });


        $scope.Modes = [{ 'id': '0', 'name': 'All' }, { 'id': '1', 'name': 'Scheduled Paid' },
        { 'id': '2', 'name': 'Scheduled Receivable' }, { 'id': '3', 'name': 'Advance Paid' },
        { 'id': '4', 'name': 'Advance Receivble' }, { 'id': '5', 'name': 'Pending Bills' }];

        $scope.refloantypelist = restService.Get('ReferencelistService/LOANTYPE', true).then(function (results) {
            $scope.refloantypelist = results;
        });

        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.refpaytypelist = restService.Get('ReferencelistService/PAYCATEGORY', true).then(function (results) {
            $scope.refpaytypelist = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Designations = restService.Get('ReferencelistService/DESIGNATION', true).then(function (results) {
            $scope.Designations = results;
        });

        $scope.WorkingModes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];

        $scope.Entity = {
            "id": 0,
            "parentid": 0,
            "employeeid": 0,
            "isreceivable": true,
            "amount": 0,
            "detail": null,
            "schedulemonth": new Date(),
            "salaryheaderid": 0,
            "institutionid": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "deductionconfig": [],
            "emiamount": 0,
            "noofinstallments": 0,
            "firstemi": new Date()
        }
        $scope.Employeesalaryschedule = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ShowNew = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowNew = !$scope.ShowNew;
        }

        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";

        $scope.CalculateEMI = function () {
            if ($scope.Employeesalaryschedule.amount == 0 || $scope.Employeesalaryschedule.noofinstallments == 0) return;

            $scope.Employeesalaryschedule.emiamount = Math.floor($scope.Employeesalaryschedule.amount / $scope.Employeesalaryschedule.noofinstallments);
        }

        $scope.Save = function () {

            if ($scope.Employeesalaryschedule.employeeid == null || $scope.Employeesalaryschedule.employeeid == 0) {
                $scope['setFocusemployeeid']();
                $rootScope.Toaster.Error("Please select employee");
                return;
            }
            //if ($scope.Employeesalaryschedule.schedulemonth == null || $scope.Employeesalaryschedule.schedulemonth == 0) {
            //    $scope['setFocusschedulemonth']();
            //    $rootScope.Toaster.Error("Please select schedulemonth");
            //    return;
            //}
            if ($scope.Employeesalaryschedule.amount == null || $scope.Employeesalaryschedule.amount == 0) {
                $scope['setFocusamount']();
                $rootScope.Toaster.Error("Please fill out amount");
                return;
            }
            //if ($scope.Employeesalaryschedule.firstemi == null || $scope.Employeesalaryschedule.firstemi == 0) {
            //    $scope['setFocusfirstemi']();
            //    $rootScope.Toaster.Error("Please select fromdate");
            //    return;
            //}
            if ($scope.DeductionSchedule == null || $scope.DeductionSchedule.length == 0) {
                $rootScope.ShowAlert('Create Schedule then save.', 'No Schedule available');
                return;
            }
            $scope.Employeesalaryschedule.institutionid = UserDetails.institutionid;
            $scope.Employeesalaryschedule.deductionconfig = $scope.DeductionSchedule;
            request = {
                "Item": $scope.Employeesalaryschedule,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");

                    if (typeof ($scope.Employeesalaryschedule.customerinvoiceid) != 'undefined' && $scope.Employeesalaryschedule.customerinvoiceid > 0) {
                        var ditem = $scope.SearchResult.find(function (item) {
                            return item.id == $scope.Employeesalaryschedule.customerinvoiceid;
                        });
                        if (typeof (ditem) != 'undefined' && ditem != null)
                            $scope.SearchResult.splice($scope.SearchResult.indexOf(ditem), 1);

                        $scope.ShowNew = false;
                    }

                    $scope.Employeesalaryschedule = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.DeductionSchedule = [];
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }

        $scope.ManageSearchItem = function (action, item) {
            if ($rootScope.UserInfo.p_SALARYDEDUCTION.modify || $rootScope.UserInfo.p_SALARYDEDUCTION.delete) {
                if (action == "Delete") {

                    //$rootScope.Confirm('Delete', 'Do you want to delete ' + item.name + "?", function () {

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
                                        url: "EmployeesalaryscheduleService/Delete",
                                        data: srequest
                                    }).success(function (result) {

                                        if (result) {
                                            $scope.ErrorMessage = "Successfully deleted";
                                            $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                            if (item.isreceivable) {
                                                var deletelist = [];
                                                $.each($scope.SearchResult, function (k, v) {
                                                    if (v.parentid == item.id)
                                                        deletelist.push(v);
                                                });

                                                $.each(deletelist, function (k, v) {
                                                    $scope.SearchResult.splice(v, 1);
                                                });
                                            }
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
                    $scope.DeductionSchedule = [];
                    $scope.GetItem(item);
                    $scope.Navigation.SetMode('EDIT');
                }

                else if (action == "Create") {
                    $scope.ShowNew = true;
                    $scope.DeductionSchedule = [];

                    var Firstemi = new Date(new Date(item.schedulemonth).setDate(1));
                    Firstemi = new Date(Firstemi.setMonth(Firstemi.getMonth() + 1));
                    $scope.Employeesalaryschedule = {
                        'employeeid': item.employeeid, 'salaryheaderid': item.salaryheaderid, 'detail': 'Bill No :' + item.detail, 'schedulemonth':
                            new Date(item.schedulemonth), 'amount': item.amount, 'firstemi': Firstemi, 'customerinvoiceid': item.id
                    };


                }
            }
        }

        $scope.AddNew = function () {
            $scope.Loan = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.DeductionSchedule = [];
            $scope.ShowNew = true;
        }

        $scope.GetItem = function (item) {
            request = {
                "Item": { id: item.id },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.Employeesalaryschedule = result[0];
                    if ($scope.Employeesalaryschedule.deductionconfig != null
                        && $scope.Employeesalaryschedule.deductionconfig.length > 0
                        && result.length > 0) {
                        $scope.Employeesalaryschedule.emiamount = $scope.Employeesalaryschedule.deductionconfig[0].amount;
                        $scope.Employeesalaryschedule.firstemi = $scope.Employeesalaryschedule.deductionconfig[0].schedulemonth;
                        $scope.Employeesalaryschedule.noofinstallments = $scope.Employeesalaryschedule.deductionconfig.length;
                        $scope.DeductionSchedule = $scope.Employeesalaryschedule.deductionconfig;
                    }
                }
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid, filename,subtitle) {
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

        $scope.ExtendMonth = function (item) {
            var index = $scope.DeductionSchedule.indexOf(item);
            var last = $scope.DeductionSchedule[$scope.DeductionSchedule.length - 1];
            var schedulemonth = new Date(last.schedulemonth).addMonths(1);
            $scope.DeductionSchedule.push({
                'amount': last.amount,
                'schedulemonth': new Date(schedulemonth.getFullYear(), schedulemonth.getMonth(), schedulemonth.getDate()).addMonths(i),
            });

            var lastamount = item.amount;
            var nextamount = 0;
            for (var i = index + 1; i < $scope.DeductionSchedule.length; i++) {
                nextamount = $scope.DeductionSchedule[i].amount;
                $scope.DeductionSchedule[i].amount = lastamount;
                lastamount = nextamount;
            }
            item.amount = 0;
        }


        $scope.PrintTable = function (div, title, actioncolumncount, width, height) {

            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;

            var table = $(div);
            var ctable = table.clone();


            ctable.find('INPUT').remove();

            /*var headerrows = ctable.find('THEAD TR');
            var remorows = headerrows.length - 2;
            for (var index = 0; index < remorows; index++)
            {
                headerrows[index].remove();
            }*/

            ctable.find('THEAD TR.hidden-header').remove();

            for (var index = 0; index < actioncolumncount; index++) {
                ctable.find("th:last-child").remove();
                ctable.find("td:last-child").remove();
            }

            ctable.find('.th-inner').removeClass();
            ctable.removeClass();
            ctable.addClass('fixed_table');
            ctable.addClass('table-bordered');

            var titlehr = document.createElement("<tr><td colsplan='" + ctable.find('THEAD TR TD').length + "'>" + $rootScope.UserInfo.CompanyName + "</td></tr>");
            ctable.find('THEAD').insertBefore(ctable.find('THEAD TR')[0]);

            var printContents = ctable[0].outerHTML;
            var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=" + width + ", height=" + height + ", resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            var newWin = window.open("", "_blank", winAttr);
            var writeDoc = newWin.document;
            writeDoc.open();
            writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            writeDoc.close();
            newWin.focus();

            angular.element('#apptitle').context.title = otitle;
        }

        $scope.SearchCondition = {
            employeeid: "",
        }

        $scope.Search = function () {
            var request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    floorid: $scope.floorid,
                    sectionid: $scope.sectionid,
                    companyid: $scope.companyid,
                    salaryheaderid: $scope.salaryheaderid,
                    from: $scope.from,
                    to: $scope.to,
                    showallemployee: false,
                    mode: $scope.mode,
                    salarymode: $scope.salarymode,
                    workingmode: $scope.workingmode
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.mode == 5) {
                    $.each(result, function (k, v) {
                        v.isbill = true;
                    })
                }
                $scope.SearchResult = result;
                $scope.ConfigGrid();

            }).error(function (result) {
            });
        }

        $scope.igrid = null;
        $scope.ConfigGrid = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddFormatedColumn('schedulemonth', 'Date', 100, 'dd-MM-yyyy');

            igrid.AddReferenceColumn('salaryheaderid', 'Salary Head', 120, $scope.refpaytypelist);


            var column = igrid.AddCustomColumn('Paid', 'Paid', 100, function (item, col) {
                return item.isreceivable && item.amount > 0 ? item.amount : '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.isreceivable && item.amount > 0 ? item.amount : 0;
            }

            var column = igrid.AddCustomColumn('Receivable', 'Receivable', 100, function (item, col) {
                return !item.isreceivable && item.amount > 0 ? item.amount : '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return !item.isreceivable && item.amount > 0 ? item.amount : 0;
            }

            var commands = "";
            if ($rootScope.UserInfo.p_SALARYDEDUCTION.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_SALARYDEDUCTION.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                });
            });

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }


        $scope.DeductionSchedule = [];
        $scope.Schedule = function () {
            $scope.DeductionSchedule = [];
            var amount = $scope.Employeesalaryschedule.amount;
            var fromDate = new Date($scope.Employeesalaryschedule.firstemi);
            var total = 0;
            for (i = 0; i < $scope.Employeesalaryschedule.noofinstallments; i++) {
                var dueDate = fromDate.addMonths(i);
                if (amount > (total + $scope.Employeesalaryschedule.emiamount)) {
                    $scope.DeductionSchedule.push({
                        'amount': $scope.Employeesalaryschedule.emiamount,
                        'schedulemonth': dueDate,
                    })
                    total += $scope.Employeesalaryschedule.emiamount;
                } else {
                    if (amount - total > 0) {
                        $scope.DeductionSchedule.push({
                            'amount': amount - total,
                            'schedulemonth': dueDate,
                        })
                        total += (amount - total);
                    }
                }
            }

            if (total != amount) {
                $scope.DeductionSchedule[$scope.DeductionSchedule.length - 1].amount += (amount - total);
            }
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);