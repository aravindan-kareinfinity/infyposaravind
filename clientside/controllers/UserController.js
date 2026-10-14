
var app = angular.module('InfinityLab');

app.controller('UserController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;

        $scope.Employes = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employes = results;
        });

        $scope.Roles = [{ id: 1, name: 'ADMIN' }, { id: 2, name: 'BILLER' }, { id: 3, name: 'CASHIER' }, { id: 4, name: 'SALES MANAGER' },
        { id: 5, name: 'SALES ASSIST' }, { id: 6, name: 'WH MANAGER' }, { id: 7, name: 'WH ASSIST' }, { id: 8, name: 'HR MANAGER' },
        { id: 9, name: 'HR ASSIST' }, { id: 10, name: 'STORE KEEPER' }, { id: 11, name: 'ACCOUNTANT' }];

        $scope.Landingscreens = [{ id: 'Dashboard', name: 'Dashboard' }, { id: 'LR Inbox', name: 'LR Inbox' }, { id: 'Sales', name: 'Sales' },
        { id: 'Settlement', name: 'Settlement' }, { id: 'Employee In-Out', name: 'Employee In-Out' }, { id: 'HR Dashboard', name: 'HR Dashboard' },
        { id: 'Intray', name: 'Intray' }, { id: 'Work Order Inbox', name: 'Work Order Inbox' },
        { id: 'Sales Order Inbox', name: 'Sales Order Inbox' }, { id: 'MfgDashboard', name: 'Mfg Dashboard' },
        { id: 'Notification', name: 'Notification' }, { id: 'Feedback', name: 'Feedback Collection' },
        { id: 'Catalog', name: 'Catalog' }];

        $scope.permissionlist = [];
        $scope.refpermission = restService.Get('ReferencelistService/PERMISSION', true).then(function (results) {
            $scope.refpermission = results;
            var pl = {};
            $.each(results, function (k, v) {
                if (typeof (pl[v.parent]) == 'undefined') {
                    pl[v.parent] = [];
                    $scope.permissionlist.push({
                        'name': v.parent, list: pl[v.parent], 'showdetail': false
                    });
                }
                pl[v.parent].push(v);
            })
        });

        $scope.refspppermission = restService.Get('ReferencelistService/SPECIALPERMISSION', true).then(function (results) {
            $scope.refspppermission = results;
        });

        $scope.refdatpermission = restService.Get('ReferencelistService/DATAPERMISSION', true).then(function (results) {
            $scope.refdatpermission = results;
        });

        $scope.refsection = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.refsection = results;
        });

        $scope.reflocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.reflocation = results;
        });
        $scope.refcompany = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.refcompany = results;
        });
        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
        });
        $scope.allview = false;
        $scope.allcreate = false;
        $scope.allmodify = false;
        $scope.alldelete = false;
        $scope.allprint = false;
        $scope.alllocation = false;
        $scope.allspecial = false;
        $scope.allsection = false;
        $scope.allcompany = false;
        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "security": {},
            "modifiedby": 0,
            "version": 1,
            "createdon": 0,
            "modifiedon": 0,
            "username": '',
            "permissions": [],
            "accessiblelocation": [],
            "security": {
                'iprestriction': false, 'locationrestriction': false, 'biometric': false, 'timerestriction': false,
                'from': '', 'to': '', 'globaluser': false
            },
            "password": '',
            "actualuserid": 0,
            "externalprovider": null,
            "role": 0,
            "landingscreen": null,
            "employeeid": 0,
            "changepassword": false,
            "lastloginon": 0,
            "roleName": 0
        };


        $scope.isCompanyTab = false;
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.ErrorMessage = "";

        $scope.GetUserRole = function () {

            request = {
                "Item": { type: 'USERROLE', isactive: true },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/Search",
                data: srequest
            }).success(function (result) {
                $scope.refuserrole = result;
            }).error(function (result) {
            });

        }

        $scope.SearchCondition = {
            employeeid: "",
            username: "",
            role: ""
        }
        $scope.New = function () {
            $scope.User = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.allview = false;
            $scope.allcreate = false;
            $scope.allmodify = false;
            $scope.alldelete = false;
            $scope.allprint = false;
            $scope.alllocation = false;
            $scope.allsection = false;
            $scope.allspecial = false;
            $scope.allcompany = false;

            $.each($scope.refpermission, function (k, v) {
                v.view = false;
                v.create = false;
                v.modify = false;
                v.delete = false;
                v.print = false;
            });

            $.each($scope.refspppermission, function (k, v) {
                v.view = false;
                v.create = false;
                v.modify = false;
                v.delete = false;
                v.print = false;
            });

            $.each($scope.refdatpermission, function (k, v) {
                v.view = false;
                v.create = false;
                v.modify = false;
                v.delete = false;
                v.print = false;
            });

            $.each($scope.reflocation, function (k, v) {
                v.view = false;
            });
            $.each($scope.refsection, function (k, v) {
                v.view = false;
            });
            $.each($scope.refcompany, function (k, v) {
                v.view = false;
            });

            //if ($scope.ShowSearch)
            //    $scope.Search();
        }

        $scope.GenerateAccessKey = function () {
            var request = {
                "Item": {
                    'id': $scope.User.id, 'version': $scope.User.version
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "UsersService/GenerateAccessKey",
                data: srequest
            }).success(function (result) {

                if (result != null) {
                    $rootScope.Toaster.Info("User Saved Successfully.!");
                    $scope.User.version = result.version;
                    $scope.User.accesskey = result.accesskey;
                }

            }).error(function (result) {
            });
        }

        $scope.Save = function () {
            /*if ($scope.User.employeeid == null || $scope.User.employeeid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Employee Name");
                return;
            }*/

            if ($scope.User.username == null || $scope.User.username == '') {
                $scope['setFocususername']();
                $rootScope.Toaster.Error("Please Fill Out User Name");
                return;
            }
            if (($scope.User.id == 0 || $scope.User.changepassword)) {

                if (($scope.User.password == null || $scope.User.password == '')) {
                    if (typeof ($scope['setFocuspassword']) != 'undefined') {
                        $scope['setFocuspassword']();
                        $rootScope.Toaster.Error("Please Fill  Out Password");
                        return;
                    }
                }

                if (($scope.User.confirmpassword == null || $scope.User.confirmpassword == '')) {
                    if (typeof ($scope['setFocusconfirmpassword']) != 'undefined') {
                        $scope['setFocusconfirmpassword']();
                        $rootScope.Toaster.Error("Please Fill  Out Confirm Password");
                        return;
                    }
                }

                if (($scope.User.password != $scope.User.confirmpassword)) {
                    if (typeof ($scope['setFocusconfirmpassword']) != 'undefined') {
                        $scope['setFocusconfirmpassword']();
                        $rootScope.Toaster.Error("Confirm password mismatching.");
                        return;
                    }
                }


            }
            if ($scope.User.role == null || $scope.User.role == 0) {
                $scope['setFocusrole']();
                $rootScope.Toaster.Error("Please Select Role");
                return;
            }


            request = {
                "Item": $scope.User,
                "securitytoken": "String content"
            }

            $scope.User.permissions = [];
            $scope.User.accessiblelocation = [];
            $scope.User.accessablesections = [];
            $scope.User.accessiblecompany = [];

            $.each($scope.refpermission, function (k, v) {
                $scope.User.permissions.push(v);
            });

            $scope.User.security.globaluser = false;
            $scope.User.security.timerestriction = false;
            $scope.User.security.iprestriction = false;
            $scope.User.security.locationrestriction = false;
            $scope.User.security.biometric = false;
            $.each($scope.refspppermission, function (k, v) {
                v.create = v.view;
                v.modify = v.view;
                if (v.view && v.code == 'SECURITY_GLOBALUSER')
                    $scope.User.security.globaluser = true;
                $scope.User.permissions.push(v);
            });

            $.each($scope.refdatpermission, function (k, v) {

                v.create = v.view;
                v.modify = v.view;
                if (v.view && v.code == 'SECURITY_TIME')
                    $scope.User.security.timerestriction = true;
                else if (v.view && v.code == 'SECURITY_IP')
                    $scope.User.security.iprestriction = true;
                else if (v.view && v.code == 'SECURITY_LOCATION')
                    $scope.User.security.locationrestriction = true;
                else if (v.view && v.code == 'SECURITY_BIOMETRIC')
                    $scope.User.security.biometric = true;
                $scope.User.permissions.push(v);
            });

            $.each($scope.reflocation, function (k, v) {
                if (v.view)
                    $scope.User.accessiblelocation.push(v);
            });

            if ($scope.User.alllocation ||
                $scope.User.accessiblelocation.length == $scope.reflocation.length) {
                $scope.User.accessiblelocation.push({ 'id': -1, 'view': true });
            }

            $.each($scope.refcompany, function (k, v) {
                if (v.view)
                    $scope.User.accessiblecompany.push(v);
            });

            if ($scope.User.accessiblecompany != 'undefined' && $scope.User.accessiblecompany != null &&
                ($scope.User.allcompany || $scope.User.accessiblecompany.length == $scope.refcompany.length)) {
                $scope.User.accessiblecompany.push({ 'id': -1, 'view': true });
            }

            $.each($scope.refsection, function (k, v) {
                if (v.view)
                    $scope.User.accessablesections.push(v);
            });

            if ($scope.User.allsection ||
                $scope.User.accessablesections.length == $scope.refsection.length)
                $scope.User.accessablesections.push({ 'id': -1, 'view': true });

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "UsersService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("User Saved Successfully.!");
                    $scope.ErrorStyle = "success";
                    //$scope.User = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }

        $scope.RoleBasedPermission = function () {

            var role = $scope.refuserrole.find(function (item) {
                return item.code == $scope.User.roleprofileid;
            });

            if (role != null && role.additionalinfo.permissionset != null) {
                $.each($scope.refpermission, function (k, v) {
                    var permission = role.additionalinfo.permissionset.find(function (item) {
                        return item.code == v.code;
                    });

                    if (permission != null) {
                        v.view = true;
                        v.create = true;
                        v.modify = true;
                        v.delete = true;
                        v.print = true;
                    }
                    else {
                        v.view = false;
                        v.create = false;
                        v.modify = false;
                        v.delete = false;
                        v.print = false;
                    }
                });

            }
        };

        /*$scope.Clear = function () {
            $scope.User = (JSON.parse(JSON.stringify($scope.Entity)));
            $.each($scope.refpermission, function (k, v) {
                
                    v.view =false;
                    v.create = false;
                    v.modify = false;
                    v.delete = false;
                    v.print = false;
               
            });

            $.each($scope.refpermission, function (k, v) {

                v.view = false;
                v.create = false;
                v.modify = false;
                v.delete = false;
                v.print = false;
            });
        };*/

        $scope.Exist = function (list, rule) {
            var found = list.find(rule);
            return typeof (found) != 'undefined' && found != null;
        }

        $scope.ManageSearchItem = function (action, item) {

            if (action == "Kill Session") {
                request = {
                    "Item": { 'userid': item.id },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "UsersessionService/KillUser",
                    data: srequest
                }).success(function (result) {
                    if (result) {
                        $scope.ErrorMessage = "Successfully Killed";
                    }
                }).error(function (result) {

                });
            }
            else if (action == "Delete") {


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
                                    url: "UsersService/Delete",
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
                $scope.Navigation.SetMode('EDIT');
                $scope.New();
                $scope.User = item;
                if (item.security == null) item.security = {};
                if ($scope.User.permissions == null)
                    $scope.User.permissions = [];

                if ($scope.User.accessiblelocation == null)
                    $scope.User.accessiblelocation = [];

                if ($scope.User.accessiblecompany == null)
                    $scope.User.accessiblecompany = [];

                $.each($scope.refpermission, function (k, v) {
                    var sub = $scope.User.permissions.find(function (item) {
                        return item.code == v.code;
                    });
                    if (sub != null) {
                        v.view = sub.view;
                        v.create = sub.create;
                        v.modify = sub.modify;
                        v.delete = sub.delete;
                        v.print = sub.print;
                    }
                });

                $.each($scope.permissionlist, function (k, v) {
                    v.view = !$scope.Exist(v.list, function (item) { return item.view == false });
                    v.create = !$scope.Exist(v.list, function (item) { return item.create == false });
                    v.modify = !$scope.Exist(v.list, function (item) { return item.modify == false });
                    v.delete = !$scope.Exist(v.list, function (item) { return item.delete == false });
                    v.print = !$scope.Exist(v.list, function (item) { return item.print == false });
                });

                $scope.allview = !$scope.Exist($scope.permissionlist, function (item) { return item.view == false });
                $scope.allcreate = !$scope.Exist($scope.permissionlist, function (item) { return item.create == false });
                $scope.allmodify = !$scope.Exist($scope.permissionlist, function (item) { return item.modify == false });
                $scope.alldelete = !$scope.Exist($scope.permissionlist, function (item) { return item.delete == false });
                $scope.allprint = !$scope.Exist($scope.permissionlist, function (item) { return item.print == false });

                $scope.User.alllocation = true;
                $scope.User.allsection = true;
                $scope.User.allspecial = true;
                $scope.User.alldata = true;
                $scope.User.allsection = true;
                $scope.User.allcompany = true;

                $.each($scope.refspppermission, function (k, v) {
                    var sub = $scope.User.permissions.find(function (item) {
                        return item.code == v.code;
                    });
                    if (typeof (sub) != 'undefined' && sub != null && sub.view) {
                        v.view = true;
                    }
                    else {
                        $scope.User.allspecial = false;
                    }
                });

                $.each($scope.refdatpermission, function (k, v) {
                    var sub = $scope.User.permissions.find(function (item) {
                        return item.code == v.code;
                    });
                    if (typeof (sub) != 'undefined' && sub != null && sub.view) {
                        v.view = true;
                    }
                    else {
                        $scope.User.alldata = false;
                    }
                });

                $.each($scope.reflocation, function (k, v) {
                    var sub = $scope.User.accessiblelocation.find(function (item) {
                        return item.id == v.id;
                    });
                    if (typeof (sub) != 'undefined' && sub != null && sub.view) {
                        v.view = true;
                    }
                    else {
                        $scope.User.alllocation = false;
                    }
                });

                $.each($scope.refcompany, function (k, v) {
                    var sub = $scope.User.accessiblecompany.find(function (item) {
                        return item.id == v.id;
                    });
                    if (typeof (sub) != 'undefined' && sub != null && sub.view) {
                        v.view = true;
                    }
                    else {
                        $scope.User.allcompany = false;
                    }
                });

                $.each($scope.refsection, function (k, v) {
                    var sub = $scope.User.accessablesections.find(function (item) {
                        return item.id == v.id;
                    });
                    if (typeof (sub) != 'undefined' && sub != null && sub.view) {
                        v.view = true;
                    }
                    else {
                        $scope.User.allsection = false;
                    }
                });
            }
        }

        $scope.SelectAllPermission = function (v, action, value) {
            if (action == 'View') {
                $.each(v.list, function (kk, vv) {
                    vv.view = value;
                });
            }
            else if (action == 'Create') {
                $.each(v.list, function (kk, vv) {
                    vv.create = value;
                });
            } else if (action == 'Modify') {

                $.each(v.list, function (kk, vv) {
                    vv.modify = value;
                });
            } else if (action == 'Delete') {
                $.each(v.list, function (kk, vv) {
                    vv.delete = value;
                });
            } else if (action == 'Print') {
                $.each(v.list, function (kk, vv) {
                    vv.print = value;
                });
            }
        }


        $scope.SelectAll = function (action) {
            if (action == 'View') {
                $.each($scope.permissionlist, function (k, v) {
                    v.view = $scope.allview;
                    $.each(v.list, function (kk, vv) {
                        vv.view = $scope.allview;
                    });
                })
            }
            else if (action == 'Create') {
                $.each($scope.permissionlist, function (k, v) {
                    v.create = $scope.allcreate;
                    $.each(v.list, function (kk, vv) {
                        vv.create = $scope.allcreate;
                    });
                })
            } else if (action == 'Modify') {
                $.each($scope.permissionlist, function (k, v) {
                    v.modify = $scope.allmodify;
                    $.each(v.list, function (kk, vv) {
                        vv.modify = $scope.allmodify;
                    });
                })
            } else if (action == 'Delete') {
                $.each($scope.permissionlist, function (k, v) {
                    v.delete = $scope.alldelete;
                    $.each(v.list, function (kk, vv) {
                        vv.delete = $scope.alldelete;
                    });
                })
            } else if (action == 'Print') {
                $.each($scope.permissionlist, function (k, v) {
                    v.print = $scope.allprint;
                    $.each(v.list, function (kk, vv) {
                        vv.print = $scope.allprint;
                    });
                })
            }
            else if (action == 'Special') {
                $.each($scope.refspppermission, function (k, v) {
                    v.view = $scope.allspecial;
                })
            } else if (action == 'Location') {
                $.each($scope.reflocation, function (k, v) {
                    v.view = $scope.User.alllocation;
                })
            } else if (action == 'Section') {
                $.each($scope.refsection, function (k, v) {
                    v.view = $scope.User.allsection;
                })
            } else if (action == 'Company') {
                $.each($scope.refcompany, function (k, v) {
                    v.view = $scope.User.allcompany;
                })
            }
            else if (action == 'Data') {
                $.each($scope.refdatpermission, function (k, v) {
                    v.view = $scope.alldata;
                })
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "UsersService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.UserConfig();
            }).error(function (result) {
            });

        }

        $scope.ShowCustomerAccess = function () {
            var dialog = ngDialog.open({
                template: 'CustomerList',
                data: [$scope, $scope.User],
                width: '600px',
                controller: ['$scope', '$timeout', 'restService', '$rootScope', function ($scope, $timeout, restService, $rootScope) {
                    $scope.SearchRef = restService.SearchReference;
                    $scope.ParentScope = $scope.ngDialogData[0];
                    $scope.User = $scope.ngDialogData[1];
                    $scope.Customers = [];
                    $scope.accessiblecustomer = [];
                    $scope.accessiblecustomerSorted = [];
                    restService.Get('ReferencelistService/CUSTOMERLEDGERUNAPPROVED', true).then(function (results) {
                        $scope.Customers = results;
                    });
                    restService.Get('UsersService/MyCustomer/' + $scope.User.id, true).then(function (results) {
                        $scope.accessiblecustomer = results.accessiblecustomer;
                        $scope.ShowSortedCustomers();
                    });

                    $scope.ShowSortedCustomers = function () {
                        $scope.accessiblecustomerSorted = $scope.accessiblecustomer
                            .map(item => ({
                                original: item,
                                searchValue: $scope.SearchRef(item, $scope.Customers)
                            }))
                            .sort((a, b) => a.searchValue.localeCompare(b.searchValue))
                            .map(item => item.original);
                    }
                    $scope.AddCustomer = function (customerid) {
                        if ($scope.accessiblecustomer.indexOf(customerid) < 0) {
                            $scope.accessiblecustomer.push(customerid);
                            $scope.ShowSortedCustomers();
                        }
                        else {
                            $rootScope.Toaster.Info('This customer already in the accessable list');
                        }
                    }
                    $scope.RemoveCustomer = function (customerid) {
                        $scope.accessiblecustomer.splice($scope.accessiblecustomer.indexOf(customerid), 1);
                        $scope.ShowSortedCustomers();
                    }

                    $scope.Selected = function () {
                        request = {
                            "Item": { 'id': $scope.User.id, 'accessiblecustomer': $scope.accessiblecustomer },
                            "SecurityToken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "UsersService/MyCustomer",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog('Successfully saved');
                        }).error(function (result) {
                        });
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $rootScope.Toaster.Info(data.value);
                    }
                }
            });
        }

        $scope.SetGlobalUser = function (globalpermission) {
            var request = {
                "Item": { 'ids': [], 'globalpermission': globalpermission },
                "SecurityToken": "String content"
            }

            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                request.Item.ids.push(v.id);
            });

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "UsersService/SetGlobalPermission",
                data: srequest
            }).success(function (result) {
                $scope.closeThisDialog('Successfully saved');
            }).error(function (result) {
            });
        }

        $scope.UserConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            $scope.selectedcount = 0;
            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.SearchResult);
            igrid.AddReferenceColumn('employeeid', 'Employee', 200, $scope.Employes);
            igrid.AddSimpleColumn('username', 'Login Name', 200);
            igrid.AddReferenceColumn('role', 'Role', 120, $scope.Roles);
            igrid.AddReferenceColumn('landingscreen', 'Landing Page', 150, $scope.Landingscreens);



            if ($rootScope.UserInfo.p_USERMASTER.modify || $rootScope.UserInfo.p_USERMASTER.delete) {
                var commands = "";
                if ($rootScope.UserInfo.p_USERMASTER.modify)
                    commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
                if ($rootScope.UserInfo.p_USERMASTER.delete)
                    commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
                if ($rootScope.UserInfo.p_USERMASTER.delete)
                    commands += "<input type='button' class='btn btn-danger btn-xs' value='Kill Session'/>"

                igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Kill Session')
                            $scope.ManageSearchItem('Kill Session', args.item);
                    });
                });
            }

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }



        $scope.New();
        $scope.GetUserRole();
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);