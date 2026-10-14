
var app = angular.module('InfinityLab');

app.controller('incrementmanagementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog','$timeout','Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, Excel) {
        $rootScope.AccordionName = $location.path();

        $scope.CurrentDA = 0;
        $scope.CurrentBasicPay = 0;
        $scope.SearchRef = restService.SearchReference;
        $scope.refemployeelist = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.refemployeelist = results;
        });
        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });
        $scope.refdepartmentlist = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.refdepartmentlist = results;
        });
        $scope.refstructurelist = restService.Get('ReferencelistService/FULL-STRUCTURE', true).then(function (results) {
            $scope.refstructurelist = results;
        });

        $scope.MonthList = [{ 'id': 1, 'name': 'JANUARY' }, { 'id': 2, 'name': 'FEBRUARY' },
                   { 'id': 3, 'name': 'MARCH' }, { 'id': 4, 'name': 'APRIL' },
                   { 'id': 5, 'name': 'MAY' }, { 'id': 6, 'name': 'JUNE' },
                   { 'id': 7, 'name': 'JULY' }, { 'id': 8, 'name': 'AUGUST' },
                   { 'id': 9, 'name': 'SEPTEMBER' }, { 'id': 10, 'name': 'OCTOBER' },
                   { 'id': 11, 'name': 'NOVEMBER' }, { 'id': 12, 'name': 'DECEMBER' }];

        $scope.WorkingModes = [ { 'id': 0, 'name': 'ALL' },{ 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];
        
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
            "salarystructureid":0,
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

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'SALARY MANAGEMENT REPORT');
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.fileuploadedcomplete = function(value)
        {
            $.each(value.rows, function (k, v) {

                try{
                    var code = v.values[0];
                    var vcode = parseInt(code);
                    var gross = v.values[1];

                    var existingshhead = $scope.SearchResult.find(function (shitem) {
                        return shitem.employeecode == code || shitem.employeecode == vcode;
                    });

                    if (existingshhead != null)
                        existingshhead.ngrosspay = parseInt(gross);
                }catch(e)
                {

                }
            });
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
                controller: ['$scope', '$timeout', '$http' ,  function ($scope, $timeout, $http) {

                    $scope.Entity = $scope.ngDialogData[0].Entity;
                    $scope.Loaded = false;
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

                    
                    $scope.SetPayCategory = function (type, value, structure)
                    {
                        var exist = $scope.ngDialogData[0].refpaycategorylist.find(function (item) {
                            return item.additionalinfo.paytype == type;
                        });

                        if(exist != null)
                        {
                            var itm = $scope.Item.configuration.find(function (vv) {
                                return (exist.code == vv.code);
                            });

                            itm.formula = value.toString();
                        }
                    }

                    $scope.StructureChanged = function(list,id)
                    {
                        var itm = list.find(function (vv) {
                            return (vv.id==id);
                        });

                        if(itm != null)
                        {
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

                            //auto populat.
                            var gross = item.grosspay;
                            var specialdeduction = 0;;
                            var conveyance = 0;
                            var hra = 0;
                            var specialallow = 0;
                            var otherallowance = 0;
                            var available = 0;
                            if (structure.fixedbasic > 1000) {
                                if (structure.fixedbasic + structure.fixedda > gross) {
                                    specialdeduction = (structure.fixedbasic + structure.fixedda) - gross;
                                    if ((structure.fixedbasic + structure.fixedda) < 6800) {
                                        specialallow = 6800 - (structure.fixedbasic + structure.fixedda);
                                        specialdeduction += specialallow;
                                    }
                                }
                                else {
                                    available = gross - (structure.fixedbasic + structure.fixedda);
                                }


                                if (gross > 6800) {
                                    if (available > 1600)
                                        conveyance = 1600;
                                    else
                                        conveyance = available;
                                    available -= conveyance;
                                }
                                //conveyance - above 6500

                                if (gross > 7500) {
                                    hra = Math.ceil((structure.fixedbasic + structure.fixedda) * 30 / 100);
                                    if (hra > available)
                                        hra = available;

                                    if (hra > 4200)
                                        hra = 4200;
                                    available -= hra;
                                }

                                if (specialdeduction == 0 && available > 0 && gross < 6800) {
                                    specialallow = available;
                                    available -= specialallow;
                                }

                                if (available > 0) {
                                    otherallowance = available;
                                    available = 0;
                                }
                            }

                            $scope.SetPayCategory('BasicPay', structure.fixedbasic);
                            $scope.SetPayCategory('DearnessAllowance', structure.fixedda);
                            $scope.SetPayCategory('Conveyance', conveyance);
                            $scope.SetPayCategory('HouseRentAllowance', hra);
                            $scope.SetPayCategory('OtherAllowance', otherallowance);
                            $scope.SetPayCategory('SpecialAllowance', specialallow);
                            $scope.SetPayCategory('SpecialDeduction', specialdeduction);

                            if (!item.eligibleesi)
                            {
                                $scope.SetPayCategory('ESI', 0);
                            }
                            if (!item.eligiblepf)
                            {
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
                                    if (itm != null)
                                    {
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
                        $rootScope.Toaster.Info("Salary Managed Successfully.!");
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
    
                            item.grosspay = result.grosspay;
                            item.basicpay = result.basicpay;
                            item.salaryheaderid = result.salaryheaderid;

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
            showconfig: true,
            showhasleft:false
        }


        $scope.SetPayCategory = function (item,type, value, structure) {
            var exist = $scope.refpaycategorylist.find(function (item) {
                return item.additionalinfo.paytype == type;
            });

            if (exist != null) {
                var itm = item.configuration.find(function (vv) {
                    return (exist.code == vv.code);
                });

                itm.formula = value.toString();
            }
        }


        $scope.MultipleDelete = function()
        {
            var item = {};
            item.fromdate = $scope.fromdate;
            item.hasarriyar = $scope.hasarriyar;
            item.applyfrom = null;
            if ($scope.hasarriyar)
                item.applyfrom = new Date($scope.applyfromyear, $scope.applyfrommonth, 1);

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EmployeesalaryheaderService/MultipleDelete",
                data: srequest
            }).success(function (result) {

                $rootScope.Toaster.Info(result.toString()+" items Deleted Successfully.!");

            }).error(function (result) {
            });
        }

        $scope.Save = function () {

            var applyfrom = null;
            if ($scope.hasarriyar)
                applyfrom = new Date($scope.applyfromyear, $scope.applyfrommonth, 1);


            var specialallowancelimit = 6800;
            var conveyancelimit = 1600;
            var hralimit = 4200;
            var hrleligiblelimit = 7500;

            $.each($scope.SearchResult,function(k,item)
            {
                item.status = '';
                if ((typeof (item.selected) != 'undefined' && item.selected &&
                    typeof (item.ngrosspay) != 'undefined' && item.ngrosspay > 0)
                    || ($scope.hasstructurechanged && item.salarystructureid == $scope.salarystructureid)) {

                    var nItem = (JSON.parse(JSON.stringify(item)));
                    item.status = 'Calculating...';
                    nItem.salaryheaderid = 0;
                    nItem.fromdate = $scope.fromdate;
                    nItem.hasarriyar = $scope.hasarriyar;
                    nItem.applyfrom = applyfrom;
                    nItem.lastgross = item.grosspay;
                    nItem.grosspay = ($scope.hasstructurechanged && item.salarystructureid == $scope.salarystructureid) ? item.grosspay : item.ngrosspay;
                    var structure = null;
                    $.each($scope.refstructurelist, function (k, v) {
                        if (structure == null && v.id == nItem.salarystructureid) {
                            structure = v;
                        }
                    });

                    if (structure != null) {
                        nItem.basicpay = structure.fixedbasic;
                        nItem.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));

                        $.each(nItem.configuration, function (k, v) {
                            var itm = structure.configuration.find(function (vv) {
                                return (v.code == vv.code);
                            });
                            if (itm != null) {
                                v.formula = itm.formula;
                                v.code = itm.code;
                            }
                        });

                        //auto populat.
                        var gross = nItem.grosspay;
                        var specialdeduction = 0;;
                        var conveyance = 0;
                        var hra = 0;
                        var specialallow = 0;
                        var otherallowance = 0;
                        var available = 0;
                        if (structure.fixedbasic > 1000) {
                            if (structure.fixedbasic + structure.fixedda > gross) {
                                specialdeduction = (structure.fixedbasic + structure.fixedda) - gross;
                                if ((structure.fixedbasic + structure.fixedda) < specialallowancelimit) {
                                    specialallow = specialallowancelimit - (structure.fixedbasic + structure.fixedda);
                                    specialdeduction += specialallow;
                                }
                            }
                            else {
                                available = gross - (structure.fixedbasic + structure.fixedda);
                            }


                            if (gross > specialallowancelimit) {
                                if (available > conveyancelimit)
                                    conveyance = conveyancelimit;
                                else
                                    conveyance = available;
                                available -= conveyance;
                            }
                            //conveyance - above 6500

                            if (gross > hrleligiblelimit) {
                                hra = Math.ceil((structure.fixedbasic + structure.fixedda) * 30 / 100);
                                if (hra > available)
                                    hra = available;

                                if (hra > hralimit)
                                    hra = hralimit;
                                available -= hra;
                            }

                            if (specialdeduction == 0 && available > 0 && gross < specialallowancelimit) {
                                specialallow = available;
                                available -= specialallow;
                            }

                            if (available > 0) {
                                otherallowance = available;
                                available = 0;
                            }
                        }

                        $scope.SetPayCategory(nItem, 'BasicPay', structure.fixedbasic);
                        $scope.SetPayCategory(nItem, 'DearnessAllowance', structure.fixedda);
                        $scope.SetPayCategory(nItem, 'Conveyance', conveyance);
                        $scope.SetPayCategory(nItem, 'HouseRentAllowance', hra);
                        $scope.SetPayCategory(nItem, 'OtherAllowance', otherallowance);
                        $scope.SetPayCategory(nItem, 'SpecialAllowance', specialallow);
                        $scope.SetPayCategory(nItem, 'SpecialDeduction', specialdeduction);

                        if (!nItem.eligibleesi) {
                            $scope.SetPayCategory(nItem, 'ESI', 0);
                        }
                        if (!nItem.eligiblepf) {
                            $scope.SetPayCategory(nItem, 'ProvidentFund', 0);
                        }
                        if (!nItem.esiselfpaid) {
                            $scope.SetPayCategory(nItem, 'ESI_Employee', 0);
                        }
                        if (!nItem.pfselfpaid) {
                            $scope.SetPayCategory(nItem, 'PF_Employee', 0);
                        }

                        nItem.id = 0;
                        request = {
                            "Item": nItem,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);
                        item.status = 'Pending...';
                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/Save",
                            data: srequest
                        }).success(function (result) {

                            var existingshhead = $scope.SearchResult.find(function (shitem) {
                                return shitem.employeeid == result.employeeid;
                            });
                            if(result.id>0)
                            {
                                
                                if(existingshhead != null)
                                    existingshhead.status = 'Completed'
                            }
                            else {
                                if (existingshhead != null)
                                    existingshhead.status = 'Error'
                            }

                        }).error(function (result) {
                        });
                    }
                }
            })           
        }
      
        $scope.StructureChanged = function (list, id) {
            var itm = list.find(function (vv) {
                return (vv.id == id);
            });

            if (itm != null) {
                $scope.CurrentDA = itm.fixedda;
                $scope.CurrentBasicPay = itm.fixedbasic;
            }
        }

        $scope.SelectAll = function (checked) {
            $.each($scope.SearchResult, function (k, v) {
                v.selected = checked;
            })
        }
   
        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryheaderService/SearchAll",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);