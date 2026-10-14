
var app = angular.module('InfinityLab');

app.controller('salarymanagementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, Excel) {
        $rootScope.AccordionName = $location.path();
        $scope.ShowSaveAll = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.refemployeelist = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.refemployeelist = results;
            $scope.EmployeeBasedIncentive = $rootScope.UserInfo.appsettings.incentivemode == 6;
        });
        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });
        $scope.refdepartmentlist = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.refdepartmentlist = results;
        });
        $scope.companylist = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.companylist = results;
        });
        $scope.refstructurelist = restService.Get('ReferencelistService/FULL-STRUCTURE', true).then(function (results) {
            $scope.refstructurelist = results;
        });

        $scope.IncentiveSources = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Total Sales' },
            { 'id': 2, 'name': 'Location Sales' },
            { 'id': 3, 'name': 'Section Sales' },
            { 'id': 4, 'name': 'Floor Sales' },
            { 'id': 5, 'name': 'Team Sales' },
            { 'id': 6, 'name': 'Salesmen' },
            { 'id': 7, 'name': 'Company' }];

        $scope.TargetMinumumMethods = [
            { 'id': 0, 'name': 'No Target' },
            { 'id': 1, 'name': 'Fixed Value' },
            { 'id': 2, 'name': 'Percentage Of Target' }];

        $scope.CalculationSources = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Fixed' },
            { 'id': 2, 'name': 'Product Age Slab' },
            { 'id': 3, 'name': 'Section Age Slab' },
            { 'id': 4, 'name': 'Product Price Slab' },
            { 'id': 5, 'name': 'Section Price Slab' },
            { 'id': 6, 'name': 'Target Working Days' },
            { 'id': 7, 'name': 'Value Slab' },
            { 'id': 8, 'name': 'Company' },
            { 'id': 9, 'name': 'Product Age And Price' },
            { 'id': 10, 'name': 'Employe & Product Age' },
            { 'id': 11, 'name': 'Employee By Section' }
        ];

        $scope.ValueTypes =
            [{ 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Value' },
            { 'id': 2, 'name': 'Quantity' }];

        $scope.TargetMethods = [
            { 'id': 0, 'name': 'No Target' },
            { 'id': 1, 'name': 'Fixed' },
            { 'id': 2, 'name': 'No Of Workings X Gross' },
            { 'id': 3, 'name': 'Fixed Days X Gross' }];

        $scope.WorkingModes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' },
        { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': -1, 'name': 'ALL' }, { 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];
        $scope.UploadingStructure = false;
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

        $scope.refpaycategorylist = restService.Get('ReferencelistService/FULL-PAYCATEGORY', true).then(function (results) {
            $scope.refpaycategorylist = results;

            $.each(results, function (k, v) {
                $scope.Entity.configuration.push({
                    'id': v.id, 'code': v.code, 'formula':
                        v.additionalinfo == null ? '' : v.additionalinfo.formula
                });
            });
            $scope.SalaryStructure = (JSON.parse(JSON.stringify($scope.Entity)));
        });

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

        $scope.SalaryManagementOpen = function (action, item) {
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
                                item.id = item.salaryheaderid;
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "EmployeesalaryheaderService/Delete",
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

                return;
            }


            var dialog = ngDialog.open({
                template: 'SalaryManagement',
                data: [$scope, item],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {

                    $scope.Entity = $scope.ngDialogData[0].Entity;
                    $scope.Loaded = false;
                    $scope.EmployeeBasedIncentive = $scope.ngDialogData[0].EmployeeBasedIncentive;
                    $scope.IncentiveSources = $scope.ngDialogData[0].IncentiveSources;
                    $scope.CalculationSources = $scope.ngDialogData[0].CalculationSources;
                    $scope.ValueTypes = $scope.ngDialogData[0].ValueTypes;
                    $scope.TargetMinumumMethods = $scope.ngDialogData[0].TargetMinumumMethods;
                    $scope.refemployeelist = $scope.ngDialogData[0].refemployeelist;
                    $scope.Delete = function (item) {
                        request = {
                            "Item": $scope.Item,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/Delete",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(null);
                        }).error(function (result) {
                        });
                    };


                    $scope.SetPayCategory = function (type, value, structure) {
                        var exist = $scope.ngDialogData[0].refpaycategorylist.find(function (item) {
                            return item.additionalinfo.paytype == type;
                        });

                        if (exist != null) {
                            var itm = $scope.Item.configuration.find(function (vv) {
                                return (exist.code == vv.code);
                            });

                            itm.formula = value.toString();
                        }
                    }

                    $scope.StructureChanged = function (list, id) {
                        var itm = list.find(function (vv) {
                            return (vv.id == id);
                        });

                        if (itm != null) {
                            $scope.newgross = itm.fixedbasic + itm.fixedda;
                        }
                    }

                    $scope.ChangeStructure = function (item) {
                        var structure = null;
                        $.each($scope.ngDialogData[0].refstructurelist, function (k, v) {
                            if (structure == null && v.id == item.salarystructureid) {
                                structure = v;
                            }
                        });

                        if (structure != null) {
                            $scope.Item.basicpay = structure.fixedbasic;
                            $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));

                            $.each($scope.Item.configuration, function (k, v) {
                                var itm = structure.configuration.find(function (vv) {
                                    return (v.code == vv.code);
                                });
                                if (itm != null) {
                                    v.formula = itm.formula;
                                    v.code = itm.code;
                                }
                            });

                            var gross = item.grosspay;
                            if (typeof (item.ctc) == 'undefined' || item.ctc == '')
                                item.ctc = item.grosspay;

                            var ctc = item.ctc;
                            var specialdeduction = 0;;
                            var conveyance = 0;
                            var hra = 0;
                            var specialallow = 0;
                            var otherallowance = 0;
                            var available = 0;

                            if (structure.fixedbasic + structure.fixedda > ctc) {
                                specialdeduction = (structure.fixedbasic + structure.fixedda) - ctc;
                            }
                            else {
                                available = gross - (structure.fixedbasic + structure.fixedda);
                            }

                            if (available > 0) {
                                if (available > 1600)
                                    conveyance = 1600;
                                else
                                    conveyance = available;
                                available -= conveyance;
                            }

                            if (available >= Math.ceil((structure.fixedbasic + structure.fixedda) * 15 / 100)) {
                                hra = Math.ceil((structure.fixedbasic + structure.fixedda) * 30 / 100);
                                if (hra > available)
                                    hra = available;
                                available -= hra;
                            }

                            if (available > 0) {
                                specialallow = available;
                            }

                            $scope.SetPayCategory('BasicPay', structure.fixedbasic);
                            $scope.SetPayCategory('DearnessAllowance', structure.fixedda);
                            $scope.SetPayCategory('Conveyance', conveyance);
                            $scope.SetPayCategory('HouseRentAllowance', hra);
                            $scope.SetPayCategory('SpecialAllowance', specialallow);
                            $scope.SetPayCategory('SpecialDeduction', specialdeduction);

                            if (!item.eligibleesi) {
                                $scope.SetPayCategory('ESI', 0);
                            }
                            if (!item.eligiblepf) {
                                $scope.SetPayCategory('ProvidentFund', 0);
                            }

                            if (!item.esiselfpaid) {
                                $scope.SetPayCategory('ESI_Employee', 0);
                            }
                            if (!item.pfselfpaid) {
                                $scope.SetPayCategory('PF_Employee', 0);
                            }
                        }

                    }

                    if (typeof ($scope.ngDialogData[1].salaryheaderid) != 'undefined' && $scope.ngDialogData[1].salaryheaderid > 0) {
                        request = {
                            "Item": { id: $scope.ngDialogData[1].salaryheaderid },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/Search",
                            data: srequest
                        }).success(function (result) {
                            $scope.Item = result[0];
                            if ($scope.Item.configuration == null) {
                                $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));
                            }
                            else {
                                var oldconfig = $scope.Item.configuration;
                                $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));

                                $.each($scope.Item.configuration, function (k, v) {
                                    var itm = oldconfig.find(function (vv) {
                                        return (v.code == vv.code);
                                    });
                                    if (itm != null) {
                                        v.formula = itm.formula;
                                        v.code = itm.code;
                                    }
                                });
                            }
                            $scope.Loaded = true;
                        }).error(function (result) {
                        });
                    }
                    else {
                        $scope.Item = $scope.ngDialogData[1];
                        $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));
                        $scope.Loaded = true;
                    }

                    //$scope.Item = $scope.ngDialogData[1];
                    $scope.refstructurelist = $scope.ngDialogData[0].refstructurelist;
                    $scope.newsalary = function () {
                        $scope.Item.salaryheaderid = 0;
                    };

                    $scope.Change = function () {
                        $scope.closeThisDialog($scope.Item);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/Save",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Salary Managed Successfully.!");
                            item.grosspay = result.grosspay;
                            item.basicpay = result.basicpay;
                            item.salaryheaderid = result.id;
                            item.fromdate = result.fromdate;
                            item.inchargeid = result.inchargeid;
                            item.structurename = $scope.SearchRef(result.salarystructureid, $scope.refstructurelist);
                        }).error(function (result) {
                        });
                    }
                }
            });
        }


        $scope.ChangeESIPFConfig = function () {

            var seletedRows = $scope.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                retList.push({ 'salaryheaderid': v.salaryheaderid, 'employeeid': v.employeeid });
            });

            if (retList.length == 0) return;

            var dialog = ngDialog.open({
                template: 'ESIPFManagement',
                data: [$scope, retList, seletedRows[0]],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    
                    $scope.Item = {
                        ids: $scope.ngDialogData[1],
                        'changeesi': false,
                        'changepf': false,
                        'eligibleesi': $scope.ngDialogData[2].eligibleesi,
                        'eligiblepf': $scope.ngDialogData[2].eligiblepf,
                        'esiselfpaid': $scope.ngDialogData[2].esiselfpaid,
                        'pfselfpaid': $scope.ngDialogData[2].pfselfpaid
                    };
                    $scope.Change = function () {
                        $scope.closeThisDialog($scope.Item);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/SaveESIPFConfig",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Salary Managed Successfully.!");
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ChangeIncentiveConfig = function () {

            var seletedRows = $scope.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                retList.push({ 'salaryheaderid': v.salaryheaderid, 'employeeid': v.employeeid });
            });

            if (retList.length == 0) return;

            var dialog = ngDialog.open({
                template: 'IncentiveManagement',
                data: [$scope, retList, seletedRows[0]],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.Employees = $scope.ngDialogData[0].refemployeelist;
                    $scope.EmployeeBasedIncentive = $scope.ngDialogData[0].EmployeeBasedIncentive;
                    $scope.IncentiveSources = $scope.ngDialogData[0].IncentiveSources;
                    $scope.CalculationSources = $scope.ngDialogData[0].CalculationSources;
                    $scope.TargetMethods = $scope.ngDialogData[0].TargetMethods;
                    $scope.TargetMinumumMethods = $scope.ngDialogData[0].TargetMinumumMethods;
                    $scope.ValueTypes = $scope.ngDialogData[0].ValueTypes;
                    $scope.Item = {
                        ids: $scope.ngDialogData[1],
                        'incentivesource': $scope.ngDialogData[2].incentivesource,
                        'incentivecalculationsource': $scope.ngDialogData[2].incentivecalculationsource,
                        'incentivevaluetype': $scope.ngDialogData[2].incentivevaluetype,
                        'incentivepercentage': $scope.ngDialogData[2].incentivepercentage,
                        'salestargetmethod': $scope.ngDialogData[2].salestargetmethod,
                        'salestarget': $scope.ngDialogData[2].salestarget,
                        'salestargetminimummode': $scope.ngDialogData[2].salestargetminimummode,
                        'salesminimumtarget': $scope.ngDialogData[2].salesminimumtarget,
                        'incentiveratio': $scope.ngDialogData[2].incentiveratio,
                        'changeincentiveconfig': true,
                        'changesupervisor':false
                    };
                    $scope.Change = function () {
                        $scope.closeThisDialog($scope.Item);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/SaveIncentiveConfig",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Salary Managed Successfully.!");
                        }).error(function (result) {
                        });
                    }
                }
            });
        }
        $scope.SearchCondition = {
            sectionid: "",
            employeeid: "",
            departmentid: "",

        }

        $scope.SaveSalary = function () {

            request = {
                "Item": $scope.SalaryManage,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryheaderService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    $scope.ErrorStyle = "success";
                    $scope.SalaryManage = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }


        $scope.Save = function () {

            request = {
                "Item": $scope.SalaryManage,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryheaderService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    $scope.ErrorStyle = "success";
                    $scope.SalaryManage = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
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
                                    url: "EmployeesalaryheaderService/Delete",
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
                $scope.ShowSearch = false;
                $scope.SalaryManage = item;
            }
        }

        $scope.fileuploadedcomplete = function (value) {

            $scope.uploadfile = null;
            var paycategorylist = [];
            $.each($scope.refpaycategorylist, function (k, v) {
                paycategorylist[v.code] = v.id;
            });

            var result = [];
            $.each(value.rows, function (k, v) {
                try {
                    if (!(k == 0 && v.values[0] == value.columns[0])) {

                        var salaryheader = { 'configuration': [] };
                        result.push(salaryheader);

                        $.each(value.columns, function (ck, cv) {

                            if (cv == 'ID') {
                                salaryheader.salaryheaderid = v.values[ck];
                                salaryheader.id = v.values[ck];
                            }
                            else if (cv == 'EMP.ID')
                                salaryheader.employeeid = parseInt(v.values[ck]);
                            else if (cv == 'Employee Code')
                                salaryheader.employeecode = v.values[ck];
                            else if (cv == 'Name')
                                salaryheader.name = v.values[ck];
                            else if (cv == 'Structure') {
                                salaryheader.structurename = v.values[ck];
                                var paycategory = $scope.refstructurelist.find(function (paycat) {
                                    return paycat.name == salaryheader.structurename
                                });
                                salaryheader.salarystructureid = typeof (paycategory) != 'undefined' ? paycategory.id : 0;
                                //salaryheader.structurename = typeof (paycategory) != 'undefined' ? paycategory.name : '';
                            } else if (cv == 'DOJ')
                                salaryheader.dateofjoining = $scope.ParseDate(v.values[ck]);
                            else if (cv == 'Basic Pay')
                                salaryheader.basicpay = v.values[ck];
                            else if (cv == 'CTC')
                                salaryheader.ctc = v.values[ck];
                            else if (cv == 'Gross Pay')
                                salaryheader.grosspay = v.values[ck];
                            else if (cv == 'From Date')
                                salaryheader.fromdate = $scope.ParseDate(v.values[ck]);
                            else if (cv == 'Incharge') {
                                salaryheader.inchargename = v.values[ck];
                                var incharge = $scope.refemployeelist.find(function (empitem) {
                                    return empitem.name == salaryheader.inchargename
                                });
                                salaryheader.inchargeid = typeof (incharge) != 'undefined' ? incharge.id : 0;
                            }
                            else
                                salaryheader.configuration.push({ 'code': cv, 'id': paycategorylist[cv], 'formula': v.values[ck] });
                        });
                    }
                } catch (e) {

                }
            });
            $scope.ShowSaveAll = true;
            $scope.ShowResult(result);
        }

        $scope.ParseDate = function (value) {
            var myDate = new Date(value);
            //if (!isNaN(myDate.getTime()))
            //    return myDate;

            var datepart = value.toString().split(' ')[0].split('-');
            if (datepart.length != 3)
                return value;

            if (datepart[0].length == 4)
                myDate = new Date(parseInt(datepart[0]), parseInt(datepart[1])-1, parseInt(datepart[2]));

            if (datepart[2].length == 4)
                myDate = new Date(parseInt(datepart[2]), parseInt(datepart[1])-1, parseInt(datepart[0]));

            if (isNaN(myDate.getTime()))
                return new Date();

            return myDate;
        }

        $scope.SaveAll = function () {
            if ($scope.LastResult.length > 0) {

                var request = {
                    "Items": $scope.igrid.GetSelectedRows(),
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "EmployeesalaryheaderService/SaveAll",
                    data: srequest
                }).success(function (result) {
                    $scope.ShowResult(result)
                    $scope.ShowSaveAll = false;
                    $rootScope.Toaster.Info("Salary Managed Successfully.!");
                }).error(function (result) {

                });
            }
        }

        $scope.igrid = null;
        $scope.LastResult = [];
        $scope.ShowResult = function (result) {

            $scope.LastResult = result;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 50);

            $.each(result, function (k, v) {
                v.sno = k + 1;
            });

            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 80);

            igrid.AddHiddenColumn('salaryheaderid', 'ID', 0);
            igrid.AddHiddenColumn('employeeid', 'EMP.ID', 0);

            igrid.AddSimpleColumn('employeecode', 'Employee Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 100);
            igrid.AddFormatedColumn('dateofjoining', 'DOJ', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('fromdate', 'From Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('basicpay', 'Basic Pay', 80);
            igrid.AddSimpleColumn('grosspay', 'Gross Pay', 80);

            igrid.AddReferenceColumn('inchargeid', 'Incharge', 100, $scope.refemployeelist);
            igrid.AddSimpleColumn('structurename', 'Structure', 100);

            if ($scope.EmployeeBasedIncentive) {
                igrid.AddReferenceColumn('incentivesource', 'Inc.Source', 100, $scope.IncentiveSources);
                igrid.AddSimpleColumn('incentivepercentage', 'Inc %', 100);
            }



            $.each($scope.refpaycategorylist, function (k, v) {
                var column = igrid.AddCustomColumn('PS_' + k, v.code, 100, function (item, col) {
                    if (item.configuration == null) item.configuration = [];
                    var config = item.configuration.find(function (its) {
                        return col.internal.code == its.code;
                    });
                    return typeof (config) != 'undefined' && config != null ? config.formula : '';
                });
                column.internal = v;
            });

            var commands = '';
            if ($scope.UserInfo.p_SALARYMGMT.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>";
            if ($scope.UserInfo.p_SALARYMGMT.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>";

            if (commands != '') {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.SalaryManagementOpen('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.SalaryManagementOpen('Delete', args.item);

                    });
                });
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            if (result.length == 0)
                $scope.igrid.UpdateData(result);
            else
                $scope.igrid.UpdateData(result, '_id');
        }
        $scope.ShowResult([]);
        $scope.Search = function () {

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            request.Item.showconfig = true;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryheaderService/SearchAll",
                data: srequest
            }).success(function (result) {
                $scope.ShowResult(result)
                $scope.ShowSaveAll = false;
            }).error(function (result) {
            });

        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);