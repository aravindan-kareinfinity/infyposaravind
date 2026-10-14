
var app = angular.module('InfinityLab');

app.controller('LeadManagementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.location) != 'undefined')
        $scope.HasCounterSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.counter) != 'undefined')

        if (!$scope.HasLocationSelected) {
            $rootScope.Toaster.Error("Please select the location and counter");
        }

        $scope.LeadEventType = restService.Get('ReferencelistService/LEADEVENT', true).then(function (results) {
            $scope.LeadEventType = results;
        });
        $scope.LeadStatus = restService.Get('ReferencelistService/LEADSTATUS', true).then(function (results) {
            $scope.LeadStatus = results;
            $scope.LeadStatus.push({ id: -1, name: 'CLOSED' });
        });
        $scope.LeadPriority = restService.Get('ReferencelistService/LEADPRIORITY', true).then(function (results) {
            $scope.LeadPriority = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            $scope.ShowLocationInfo();
        });

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
            $scope.ShowLocationInfo();
        });

        $scope.RetailLocation = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            results.push({ 'id': 0, 'code': 'All Locations' });
            $scope.RetailLocation = results;
        });

        $scope.ShowLocationInfo = function () {
            if ($scope.HasCounterSelected) {
                $scope.LeadManagement.counterid = $rootScope.LocationInfo.counter.id;
                $scope.CounterName = $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters);
            }
            else {
                $scope.CounterName = 'SELECT COUNTER';
            }

            if ($scope.HasLocationSelected) {
                $scope.LeadManagement.locationid = $rootScope.LocationInfo.location.id;
                $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Location);
            }
            else {
                $scope.LocationName = 'SELECT LOCATION';
            }
        }

        $scope.Entity = {
            "id": 0,
            "leadno": "",
            "customerid": 0,
            "contactno": "",
            "eventtypeid": 0,
            "assignedid": 0,
            "companyid": 0,
            "locationid": 0,
            "counterid": 0,
            "priorityid": 0,
            "statusid": 0,
            "remark": "",
            "loggedon": new Date(),
            "eventon": new Date(),
            "loggedon": new Date(),
            "followupon": null,
            "isactive": true,
            "version": 1,
            "modifiedby": 0,
            "createdby": 0,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "vouchercode": "",
            "voucherid": 0,
            "billid": 0,
            "vouchertypeid": 0,
            "calllog": null,
            "coupon": null,
            "voucher": null
        };

        $scope.LeadManagement = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.CouponEntity = {
            'name': '', 'validfrom': null, 'mode': 0, 'validto': null, 'issuedby': 0, 'amount': 0, 'amountaspercentage': false, 'rules': []
        };
        $scope.Coupon = (JSON.parse(JSON.stringify($scope.CouponEntity)));
        $scope.CouponRuleEntity = {
            'mode': 0, 'acceptablelocationid': 0, 'minbillvalue': 0, 'amountaspercentage': false, 'isranged': false,
            'maxbilldiscountpercentage': 0, 'rangedrule': []
        };
        $scope.CouponRule = (JSON.parse(JSON.stringify($scope.CouponRuleEntity)));

        $scope.RangeRuleItem = { 'fromamount': 0, 'toamount': 0, 'amountaspercentage': false, 'amount': 0 };
        $scope.AddRangedRules = function () {
            if (typeof ($scope.CouponRule.rangedrule) == 'undefined' || $scope.CouponRule.rangedrule == null)
                $scope.CouponRule.rangedrule = [];
            $scope.CouponRule.rangedrule.push($scope.RangeRuleItem);
            $scope.RangeRuleItem = { 'fromamount': 0, 'toamount': 0, 'amountaspercentage': false, 'amount': 0 };
        }
        $scope.RemoveRangedRules = function (item) {
            $scope.CouponRule.rangedrule.splice($scope.CouponRule.rangedrule.indexOf(item), 1);
        }
        $scope.Addrule = function () {
            if ($scope.CouponRule.amountaspercentage) {
                if ($scope.CouponRule.amount.indexOf('%') >= 0) {
                    $scope.CouponRule.amount = $scope.CouponRule.amount.replace('%', '');
                }
            }
            $scope.CouponRule.printqty = 1; //$scope.CouponRule.quantity;

            if ($scope.CouponRule.rangedrule != 'undefined' && $scope.CouponRule.rangedrule != null
                && $scope.CouponRule.rangedrule.length == 1) {
                $rootScope.Toaster.Error("Please Use Single Rule option.");
                return;
            }
            if ($scope.CouponRule.rangedrule != 'undefined' && $scope.CouponRule.rangedrule != null
                && $scope.CouponRule.rangedrule.length > 1)
                $scope.CouponRule.isranged = true;

            if (!$scope.CouponRule.isranged) {
                if ($rootScope.GetNumber($scope.CouponRule.amount) == 0) {
                    $rootScope.Toaster.Error("Please enter discount amount/percentage");
                    return;
                }
            }
            $scope.Coupon.rules.push($scope.CouponRule);
            //$scope.CouponRule = (JSON.parse(JSON.stringify($scope.CouponRuleEntity)));
        }

        $scope.GiftEntity = {
            name: "",
            customerid: "",
            mobileno: "",
            validfrom: "",
            validto: "",
            receivedamt: "",
            vouchervalue: 0,
            issuedby: "",
            givento: "",
            isactive: true,
            version: 1,
            locationid: 0,
            denomination: [{ denomination: 0, qty: 0, amount: 0, 'iscustom': true }]
        };

        $scope.GiftVoucher = (JSON.parse(JSON.stringify($scope.GiftEntity)));

        $scope.ShowSearch = true;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.LeadManagement = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.SearchCondition = {
            "companyid": 0,
            "eventtypeid": 0,
            "priorityid": 0,
            "statusid": 0,
            "assignedid": 0,
            "fromdate": null,
            "todate": null
        }

        $scope.AmountChanged = function () {
            var oldValue = $rootScope.GetNumber($scope.GiftVoucher.denomination[0].amount);
            if ($scope.GiftVoucher.denomination[0].denomination == -1) {
                $scope.GiftVoucher.denomination[0].amount = $rootScope.GetNumber($scope.GiftVoucher.denomination[0].qty);
            }
            else {
                $scope.GiftVoucher.denomination[0].amount = isNaN($scope.GiftVoucher.denomination[0].denomination * $scope.GiftVoucher.denomination[0].qty) ? 0 : $rootScope.GetNumber($scope.GiftVoucher.denomination[0].denomination * $scope.GiftVoucher.denomination[0].qty);
            }
            $scope.GiftVoucher.vouchervalue = $rootScope.GetNumber($scope.GiftVoucher.vouchervalue) - $rootScope.GetNumber(oldValue) + $rootScope.GetNumber($scope.GiftVoucher.denomination[0].amount);
        };     

        $scope.New = function () {
            $scope.LeadManagement = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ShowSearch = false;
        }

        $scope.SearchCustomer = function () {
            if ($scope.LeadManagement.contactno == null || $scope.LeadManagement.contactno == "") {
                $rootScope.Toaster.Info("Please enter valid customer mobile no");
                $scope['setFocusfirst']();
                return;
            }
            restService.Get('CustomerService/PhoneNumber?id=' + $scope.LeadManagement.contactno, true).then(function (results) {
                if (results.id == 'undefined' || results.id == null || results.id == "") {
                    $rootScope.Toaster.Error("Can't find Customer, please refine your search or create new.");
                    $scope['setFocusfirst']();
                    return;
                }
                $scope.LeadManagement.customername = results.name;
                $scope.LeadManagement.customerid = results.id;
            });
        }

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', function ($scope, $timeout, $http, hotkeys, $rootScope) {

                    $scope.ReligionList = restService.Get('ReferencelistService/RELIGION', true).then(function (results) {
                        $scope.ReligionList = results;
                    });
                    $scope.CustomerCategoryList = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
                        $scope.CustomerCategoryList = results;
                    });
                    $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
                        $scope.CityList = results;
                    });

                    $scope.PopupList = [];
                    $scope.Customer = null;
                    $scope.CloseIfOne = false;

                    $scope.Select = function () {
                        $scope.Customer = null;
                        $.each($scope.PopupList, function (k, v) {
                            if (v.selected)
                                $scope.Customer = v;
                        });
                        if ($scope.Customer != null)
                            $scope.closeThisDialog($scope.Customer);
                    };
                    $scope.Create = function () {
                        if (!$scope.contactno || $scope.contactno == null || $scope.contactno == '') {
                            $rootScope.Toaster.Error('Please enter the customer contact no');
                            $scope.setFocuscustomer_contact();
                            return;
                        }
                        if ($scope.MobileNo10Digits && $scope.contactno.length != 10) {
                            $rootScope.Toaster.Error('Please enter 10 digits customer contact no');
                            $scope.setFocuscustomer_contact();
                            return;
                        }
                        if (!$scope.name || $scope.name == null || $scope.name == '') {
                            $rootScope.Toaster.Error('Please enter the customer name');
                            $scope.setFocuscustomer_name();
                            return;
                        }

                        if ($scope.gstno != 'undefined' && $scope.gstno != null
                            && $scope.gstno != '') {
                            var gstinformat = new RegExp('^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$');
                            if (!gstinformat.test($scope.gstno)) {
                                $scope.setFocuscustomer_gstno();
                                $rootScope.Toaster.Error("Please Enter valid GST number.");
                                return;
                            }
                        }

                        var request = {
                            "Item": {
                                'id': $scope.id, 'version': $scope.version, 'isactive': $scope.isactive,
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'registredlocationid': $rootScope.LocationInfo.location.id,
                                'religionid': $scope.religionid, 'customertype': 4,
                                'cityid': $scope.cityid, 'dateofbirth': $scope.dateofbirth,
                                'area': $scope.area, 'billingname': $scope.billingname,
                                'emailid': $scope.emailid, 'address': $scope.address,
                                'categoryid': $scope.categoryid,
                                'locationid': $rootScope.LocationInfo.location.id
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }

                    $scope.SetFocusByFlow = function (focusid) {
                        if (UserDetails.Info.p_HIDECUSTOMERCAT.view)
                            $scope['setFocus' + 'customer_religion']();
                        else
                            $scope['setFocus' + focusid]();
                        return;
                    };

                    $scope.ManageSearchItem = function (action, item) {
                        if (action == "Edit") {
                            $scope.id = item.id;
                            $scope.name = item.name;
                            $scope.area = item.area;
                            $scope.contactno = item.mobileno;
                            $scope.gstno = item.gstno;
                            $scope.hasgst = item.hasgst;
                            $scope.loyeltyno = item.loyeltyno;
                            $scope.religionid = item.religionid;
                            $scope.customertype = item.customertype;
                            $scope.cityid = item.cityid;
                            $scope.dateofbirth = item.dateofbirth;
                            $scope.area = item.area;
                            $scope.billingname = item.billingname;
                            $scope.emailid = item.emailid;
                            $scope.address = item.address;
                            $scope.categoryid = item.categoryid;
                            $scope.version = item.version;
                            $scope.isactive = item.isactive;
                        }
                    }

                    $scope.Search = function (closeIfOne) {

                        $scope.CloseIfOne = typeof (closeIfOne) != 'undefined' ? closeIfOne : false;
                        if (!$scope.hasvalue($scope.name) &&
                            !$scope.hasvalue($scope.area) &&
                            !$scope.hasvalue($scope.contactno) &&
                            !$scope.hasvalue($scope.gstno) &&
                            !$scope.hasvalue($scope.loyeltyno))
                            return;

                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'likemobileno': true,
                                'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'locationid': $rootScope.LocationInfo.location.id
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length == 1 && $scope.CloseIfOne) {
                                $scope.closeThisDialog(result[0]);
                            }
                            $scope.PopupList = result;
                        });
                    };

                    $timeout(function () {
                        //$scope.setcustomer_contact();
                    }, 10);

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document' && data.value.id > 0) {
                        $scope.LeadManagement.customerid = data.value.id;
                        $scope.LeadManagement.customername = data.value.name;
                        $scope.LeadManagement.contactno = data.value.mobileno;
                    }
                    $scope.setFocuscompanyid();
                }
            });
        }

        $scope.Save = function () {

            if ($scope.HasCounterSelected) {
                $scope.LeadManagement.counterid = $rootScope.LocationInfo.counter.id;
                $scope.CounterName = $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters);
            }

            if ($scope.LeadManagement.customerid == null || $scope.LeadManagement.customerid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please enter valid customer details");
                return;
            }
            if ($scope.LeadManagement.eventtypeid == null || $scope.LeadManagement.eventtypeid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Event Type");
                return;
            }

            if ($scope.LeadManagement.locationid == null || $scope.LeadManagement.locationid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Location");
                return;
            }
            if ($scope.LeadManagement.counterid == null || $scope.LeadManagement.counterid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Counter");
                return;
            }

            if ($scope.CouponRule.unlimitted || $rootScope.GetNumber($scope.CouponRule.quantity) > 0) {
                $scope.Addrule();
                if ($scope.Coupon.rules == 'undefined' || $scope.Coupon.rules == null || $scope.Coupon.rules.length == 0) {
                    $rootScope.Toaster.Error("Please Enter valid coupon rules.");
                    return;
                }
                $scope.LeadManagement.leadcoupon = $scope.Coupon;
            }

            if ($rootScope.GetNumber($scope.GiftVoucher.denomination[0].denomination) > 0
                && $rootScope.GetNumber($scope.GiftVoucher.denomination[0].amount)
                && $rootScope.GetNumber($scope.GiftVoucher.denomination[0].qty) > 0) {

                $scope.GiftVoucher.customerid = $scope.LeadManagement.customerid;
                $scope.GiftVoucher.mobileno = $scope.LeadManagement.contactno;
                $scope.GiftVoucher.name = $scope.LeadManagement.customername;
                $scope.LeadManagement.leadvoucher = $scope.GiftVoucher;
            }

            request = {
                "Item": $scope.LeadManagement,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LeadManagementService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Lead details Saved Successfully.!");
                    $scope.LeadManagement = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.Coupon = (JSON.parse(JSON.stringify($scope.CouponEntity)));
                    $scope.CouponRule = (JSON.parse(JSON.stringify($scope.CouponRuleEntity)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
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
                                    url: "LeadManagementService/Delete",
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
                $scope.LeadManagement = item;
                $scope.Navigation.SetMode('EDIT');
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'LEAD MANAGEMENT'
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

        $scope.PrintPDF = function (id) {
            $rootScope.Download('LeadManagementService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.LeadManagement.id : id),
                'lead.pdf', 'application/pdf');
            return;
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LeadManagementService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.LeadManagementConfig();
                $scope.igrid.UpdateData($scope.SearchResult, '_id');
            }).error(function (result) {
            });
        }

        $scope.LeadManagementConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddSimpleColumn('leadno', '#Lead Ref No', 80);
            igrid.AddSimpleColumn('customername', 'Customer', 180);
            igrid.AddSimpleColumn('contactno', 'Contact No', 120);

            var column = igrid.AddCustomColumn('Converted', 'Converted', 80, function (item, col) {
                return item.isconsumed ? 'Yes' : 'No'
            });

            igrid.AddFormatedColumn('loggedon', 'Logged On', 100, 'dd/MM/yyyy');
            igrid.AddReferenceColumn('eventtypeid', 'Type', 100, $scope.LeadEventType);
            igrid.AddReferenceColumn('priorityid', 'Priority', 80, $scope.LeadPriority);
            igrid.AddReferenceColumn('statusid', 'Status', 80, $scope.LeadStatus);
            igrid.AddReferenceColumn('assignedbyid', 'Assigned By', 150, $scope.Employee);
            igrid.AddReferenceColumn('assignedid', 'Assigned To', 150, $scope.Employee);
            igrid.AddFormatedColumn('followupon', 'Follow Up On', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('remark', 'Lead Detail', 250);

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 50, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                });
            });
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');
        }

        $timeout(function () {
            $scope.setFocusfirst();
            $scope.LeadManagementConfig();
            var tdate = new Date();
            tdate = tdate.setDate(tdate.getDate() - 7);
            $scope.SearchCondition.fromdate = $filter('date')(tdate, datePicker.shortDisplay);
            $scope.SearchCondition.todate = $filter('date')(new Date(), datePicker.shortDisplay);

            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "LeadManagementService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {
                        $scope.ShowSearch = false;
                        $scope.LeadManagement = result[0];
                    }

                }).error(function (result) {
                });
            }
            else {
                $scope.ShowSearch = true;
                $scope.Search();
            }

        }, 100);

    }]);