
var app = angular.module('InfinityLab');

app.controller('CustomerCreditNoteController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, localStorageService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.CustomerCreditNote = {
            'id': 0, 'Billcreditnoteitems': []
        };
        $scope.BillValue = 0;
        $scope.CurrentItems = [];
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
            $rootScope.BindCompany($scope, 'CustomerCreditNote.companyid');
        });

        $scope.CurrentBill = null;

        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
            $rootScope.BindLocation($scope, 'CustomerCreditNote.locationid');
        });

        $scope.ShowSearch = false;

        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.New = function () {
            $scope.CustomerCreditNote = {
                'id': 0, 'Billcreditnoteitems': []
            };
            $scope.CurrentItems = [];
            $scope.BillItemAllocations = [];
            $scope.BillValue = 0;
            $scope.CurrentBill = {};
            $scope.SearchPaymentCondition = {};
            $scope.SearchPaymentCondition.billno = '';
            $scope.SearchPaymentCondition.billpercentage = '';
            $scope.SearchPaymentCondition.billvalue = '';
            //$scope.GridConfig();
        }
        $scope.New();

        $scope.SearchPaymentCondition = {
            customerid: 0,
            showpending: true
        }

        $scope.AddedAmount = 0;
        $scope.TotalPending = 0;
        $scope.ShowCustomerDialog = function () {


            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
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
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
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
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
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
                        $scope.setFocuscustomer_name();
                    }, 10);

                    $scope.contactno = $scope.ngDialogData[0].SearchPaymentCondition.customername;
                    if (typeof ($scope.contactno) != 'undefined' && $scope.contactno != null) {
                        if ($scope.contactno.length != 10 || $scope.ngDialogData[0].GetNumber($scope.contactno) <= 0) {
                            $scope.name = $scope.contactno;
                            $scope.contactno = '';
                        }
                        $scope.Search(true);
                    }

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {




                        $scope.CurrentItems = [];
                        $scope.BillItemAllocations = [];
                        $scope.BillValue = 0;
                        $scope.SearchPaymentCondition.billno = '';
                        $scope.CurrentBill = {};
                        $scope.SearchPaymentCondition.billpercentage = '';
                        $scope.SearchPaymentCondition.billvalue = '';
                        $scope.SearchPaymentCondition.customerid = data.value.id;
                        $scope.SearchPaymentCondition.customername = data.value.name;
                        $scope.SearchCondition.customerid = data.value.id;
                        $scope.SearchCondition.customername = data.value.name;
                        $scope.CustomerCreditNote.customerid = data.value.id;
                        //$scope.SearchItems();
                    }
                }
            });
        }


        $scope.AdujstedChanged = function (item, which) {
            if (which == 'value') {
                item.adjustedtax = ($scope.GetNumber(item.adjustedvalue) * (item.taxpercentage) / 100).round(4);
            }

            item.adjustedtotal = ($scope.GetNumber(item.adjustedvalue) + $scope.GetNumber(item.adjustedtax)).round(2);
        }

        $scope.GrossAdujstedChanged = function (which) {
            if (which == 'value') {
                $scope.SearchPaymentCondition.billpercentage =
                    ($scope.GetNumber($scope.SearchPaymentCondition.billvalue) / ($scope.CurrentBill.totalreceivable) * 100).round(4);
            } else {
                $scope.SearchPaymentCondition.billvalue = ($scope.CurrentBill.totalreceivable * $scope.GetNumber($scope.SearchPaymentCondition.billpercentage) / 100).round(2);
            }

            var percentage = $scope.GetNumber($scope.SearchPaymentCondition.billpercentage);
            var totalvalue = 0;
            $.each($scope.BillItemAllocations, function (k, v) {
                v.adjustedtotal = (v.totalamount * percentage / 100).round(0);
                v.adjustedtax = (v.adjustedtotal / (v.taxpercentage + 100) * v.taxpercentage).round(2);
                v.adjustedvalue = v.adjustedtotal - v.adjustedtax;
                totalvalue += v.adjustedtotal;
            });
            if (which != 'value') {
                $scope.SearchPaymentCondition.billvalue = totalvalue;
            }
        }

        $scope.AddBill = function () {

            var bill = {
                'billid': $scope.CurrentBill.id,
                'billno': $scope.CurrentBill.billno,
                'billdate': $scope.CurrentBill.billdate,
                'customerid': $scope.CurrentBill.customerid,
                'discountpercentage': $scope.SearchPaymentCondition.billpercentage,
                'adjustedvalue': 0,
                'adjustedtax': 0,
                'adjustedtotal': 0,
                'amount': 0,
                'totalamount': 0,
                'tax': 0,
                'taxitems': []
            };

            $.each($scope.BillItemAllocations, function (k, v) {
                bill.taxitems.push(v);
                bill.adjustedvalue += v.adjustedvalue;
                bill.adjustedtax += v.adjustedtax;
                bill.adjustedtotal += v.adjustedtotal;
                bill.totalamount += v.totalamount;
                bill.tax += v.tax;
                bill.amount += v.amount;
            });

            $scope.SearchPaymentCondition.billpercentage = 0;
            $scope.SearchPaymentCondition.billvalue = 0;
            $scope.BillValue = 0;
            $scope.SearchPaymentCondition.billno = '';
            $scope.BillItemAllocations = [];
            $scope.CurrentItems.push(bill);
            $scope.GridConfig();
        }

        $scope.bgrid = null;
        $scope.GridConfig = function () {
            if ($scope.bgrid != null)
                $scope.bgrid.destroy();

            var bgrid = new InfyGrid('gridBillsContainer', '450px', '100%', true, false, true, 0);
            bgrid.options.showHeaderRow = true;

            bgrid.AddSimpleColumn('billno', 'Bill No', 150);
            bgrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
            bgrid.AddFormatedColumn('totalamount', 'Bill Value', 100, 'N2', 2).total = true;
            bgrid.AddFormatedColumn('adjustedtotal', 'Credit Note', 100, 'N2', 2).total = true;

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"

            bgrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                });
            });
            bgrid.EnableTotal();
            bgrid.Initialize();
            $scope.bgrid = bgrid;
            $scope.bgrid.UpdateData($scope.CurrentItems);
        }

        $scope.SearchBill = function () {
            var request = {
                "Item": {
                    'billno': $scope.SearchPaymentCondition.billno,
                    'customerid': $scope.SearchPaymentCondition.customerid,
                    'isimport': true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 0) {
                    $rootScope.Toaster.Info("There is no bill available for this customer");
                    return;
                }
                else {
                    $scope.ShowBill(result);
                }
            }).error(function (result) {
            });
        }

        $scope.ShowBill = function (result) {

            if ($scope.BillItemAllocations.length == 0 || typeof ($scope.SearchPaymentCondition.customerid) == 'undefined') {
                $scope.SearchPaymentCondition.customerid = result[0].customerid;
                $scope.CustomerCreditNote.customerid = result[0].customerid;
            }

            if ($scope.BillItemAllocations.length == 0 ||typeof ($scope.CustomerCreditNote.locationid) == 'undefined')
                $scope.CustomerCreditNote.locationid = result[0].locationid;

            if ($scope.BillItemAllocations.length == 0 ||typeof ($scope.CustomerCreditNote.companyid) == 'undefined')
                $scope.CustomerCreditNote.companyid = result[0].companyid;

            if ($scope.SearchPaymentCondition.customerid != result[0].customerid) {
                $rootScope.Toaster.Info("Can't change the customer");
                return;
            }
            if ($scope.CustomerCreditNote.locationid != result[0].locationid) {
                $rootScope.Toaster.Info("Can't change the location");
                return;
            }
            if ($scope.CustomerCreditNote.companyid != result[0].companyid) {
                $rootScope.Toaster.Info("Can't change the company");
                return;
            }

            if (typeof (result[0].customer) != 'undefined') {
                $scope.SearchPaymentCondition.customername = result[0].customer.name;
            }

            $scope.CurrentBill = result[0];
            $scope.CounterName = ($scope.HasLocationSelected) ? $scope.SearchRef($scope.CurrentBill.counterid, $scope.Counters) : 'Select Counter';
            $scope.LocationName = $scope.SearchRef($scope.CurrentBill.locationid, $scope.Locations);
            $scope.BillValue = $scope.CurrentBill.totalreceivable;
            $scope.BillItemAllocations = [];

            $.each($scope.CurrentBill.BillTax, function (k, v) {

                $scope.BillItemAllocations.push({
                    'taxid': v.taxid,
                    'taxpercentage': v.taxpercentage,
                    'amount': v.amount,
                    'totalamount': v.totalamount,
                    'tax': v.tax,
                    'adjustedtax': 0,
                    'adjustedvalue': 0,
                    'adjustedtotal': 0,
                    'billid': $scope.CurrentBill.id,
                    'billno': $scope.CurrentBill.billno,
                });

            });
        }
        $scope.PrintPDF = function (id) {
            $rootScope.Download('BillcreditnoteService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.LastPaymentID : id), 'customerlabel.pdf', 'application/pdf');
            return;
        }


        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };

        $scope.Save = function () {
            if ($scope.CurrentItems == null || $scope.CurrentItems.length == 0) {
                $rootScope.Toaster.Info("Please enter the amount for credit note.");
                return;
            }
            $scope.CustomerCreditNote.isactive = true;
            $scope.CustomerCreditNote.amount = 0;
            $scope.CustomerCreditNote.totalamount = 0;
            $scope.CustomerCreditNote.tax = 0;
            $scope.CustomerCreditNote.adjustedtax = 0;
            $scope.CustomerCreditNote.adjustedvalue = 0;
            $scope.CustomerCreditNote.adjustedtotal = 0;
            var alltaxitems = [];

            $.each($scope.CurrentItems, function (k, v) {
                $scope.CustomerCreditNote.amount += v.amount;
                $scope.CustomerCreditNote.totalamount += v.totalamount;
                $scope.CustomerCreditNote.tax += v.tax;
                $scope.CustomerCreditNote.adjustedtax += v.adjustedtax;
                $scope.CustomerCreditNote.adjustedvalue += v.adjustedvalue;
                $scope.CustomerCreditNote.adjustedtotal += v.adjustedtotal;
                $.each(v.taxitems, function (k, v) {
                    alltaxitems.push(v);
                });
            });

            if ($scope.CustomerCreditNote.id != 0) {
                var deleted = [];
                $.each($scope.CustomerCreditNote.Billcreditnoteitems, function (k, v) {
                    if (alltaxitems.indexOf(v) < 0) {
                        deleted.push(v);
                    }
                });

                $.each(deleted, function (k, v) {
                    $scope.CustomerCreditNote.Billcreditnoteitems.splice($scope.CustomerCreditNote.Billcreditnoteitems.indexOf(item), 1);
                });
            }

            $.each(alltaxitems, function (k, v) {
                if (typeof (v.id) == 'undefined' || v.id == 0) {
                    v.isactive = true;
                    $scope.CustomerCreditNote.Billcreditnoteitems.push(v);
                }
            })

            request = {
                "item": $scope.CustomerCreditNote,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillcreditnoteService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.ShowCreditNote(result);
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }

        $scope.SearchCondition = {
            customerid: 0,
            showpending: ""
        }
        $scope.Search = function () {
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillcreditnoteService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.CustomerCreditNoteConfig();
            }).error(function (result) {
            });

        }

        $scope.CustomerCreditNoteConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddSimpleColumn('customername', 'Customer', 250);
            igrid.AddSimpleColumn('creditnoteno', 'Credit Note No', 100);
            igrid.AddFormatedColumn('creditnotedate', 'Credit Note Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('adjustedtotal', 'Amount', 100, 'N2', 2);

            var commands = "";
            if ($rootScope.UserInfo.p_CUSTOMERCREDITNOTE.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_CUSTOMERCREDITNOTE.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            if ($rootScope.UserInfo.p_CUSTOMERCREDITNOTE.print)
                commands += "<input type='button' class='btn btn-info btn-xs' value='Print'/> "

            if ($rootScope.UserInfo.p_CUSTOMERCREDITNOTE.modify || $rootScope.UserInfo.p_CUSTOMERCREDITNOTE.delete) {
                igrid.AddButtonColumn('action', 'Action', 130, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('EditCreditNote', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('DeleteCreditNote', args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.PrintPDF(args.item.id);
                    });
                });
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.ShowCreditNote = function (item) {
            $scope.CustomerCreditNote = item;
            $scope.CurrentItems = [];
            $scope.BillItemAllocations = [];
            $scope.BillValue = 0;
            $scope.CurrentBill = {};
            $scope.SearchPaymentCondition.billno = '';
            $scope.SearchPaymentCondition.billpercentage = '';
            $scope.SearchPaymentCondition.billvalue = '';
            $scope.SearchPaymentCondition.customerid = item.customerid;
            $scope.SearchPaymentCondition.customername = item.customername;
            $scope.SearchCondition.customerid = item.customerid;
            $scope.SearchCondition.customername = item.customername;
            $.each(item.Billcreditnoteitems, function (k, v) {

                var bill = $scope.CurrentItems.find(function (citem) {
                    return citem.billid == v.billid;
                });

                if (typeof (bill) == 'undefined' || bill == null) {
                    bill = {
                        'billid': v.billid,
                        'billno': v.billno,
                        'billdate': v.billdate,
                        'customerid': item.customerid,
                        'discountpercentage': 0,
                        'adjustedvalue': 0,
                        'adjustedtax': 0,
                        'adjustedtotal': 0,
                        'amount': 0,
                        'totalamount': 0,
                        'tax': v.tax,
                        'taxitems': []
                    };
                    $scope.CurrentItems.push(bill);
                }

                bill.taxitems.push(v);
                bill.adjustedvalue += v.adjustedvalue;
                bill.adjustedtax += v.adjustedtax;
                bill.adjustedtotal += v.adjustedtotal;
                bill.totalamount += v.totalamount;
                bill.tax += v.tax;
                bill.amount += v.amount;
            });
            $scope.SwitchPanel();

            $timeout(function () {
                $scope.GridConfig();
            }, 10);
            
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $scope.CurrentItems.splice($scope.CurrentItems.indexOf(item), 1);
                $scope.bgrid.UpdateData($scope.CurrentItems);
            }
            else if (action == "Edit") {
                $scope.CurrentItems.splice($scope.CurrentItems.indexOf(item), 1);
                $scope.BillItemAllocations = item.taxitems;
                $scope.BillValue = item.totalamount;
                $scope.SearchPaymentCondition.billno = item.billno;
                $scope.CurrentBill = {
                    'id': item.billid,
                    'billno': item.billno,
                    'billdate': item.billdate,
                    'customerid': item.customerid,
                    'totalamount': item.totalamount
                };
                $scope.SearchPaymentCondition.billpercentage = item.discountpercentage;
                $scope.SearchPaymentCondition.billvalue = item.adjustedtotal;
                $scope.bgrid.UpdateData($scope.CurrentItems);
            }
            else if (action == 'DeleteCreditNote') {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ?',
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
                                    url: "BillcreditnoteService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.SearchResult);
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
            else if (action == 'EditCreditNote') {

                var request = {
                    "Item": {
                        'id': item.id
                    },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BillcreditnoteService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.ShowCreditNote(result[0]);
                    $scope.Navigation.SetMode('EDIT');
                }).error(function (result) {
                });

            }
        }

    }]);

