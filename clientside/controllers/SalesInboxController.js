
var app = angular.module('InfinityLab');

app.controller('SalesInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$filter', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $filter, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Suppliers = [];
        $scope.Company = [];
        $scope.Location = [];
        $scope.EnableDelivery = false;
        $scope.CreateDelivery = false;
        $scope.ApproveItem = null;
        $scope.igrid = null;
        $scope.SelectedCount = 0;
        $scope.TransportModes = [{ 'id': 1, 'name': 'Road' }, { 'id': 2, 'name': 'Rail' }, { 'id': 3, 'name': 'Air' }, { 'id': 4, 'name': 'Ship' }];
        $scope.StatusList = [{ 'id': -1, 'name': 'ALL' }, { 'id': 0, 'name': 'New Invoice' },
        { 'id': 1, 'name': 'Unapproved' },
        { 'id': 2, 'name': 'Approved' },
        { 'id': 4, 'name': 'Packed' },
        { 'id': 5, 'name': 'On Transit' },
        { 'id': 6, 'name': 'At Destination' },
        { 'id': 7, 'name': 'Reached' },
        { 'id': 8, 'name': 'On Return' },
        { 'id': 9, 'name': 'Returned' },
        { 'id': 10, 'name': 'Transport' },
        { 'id': 3, 'name': 'Completed' },
        { 'id': -9, 'name': 'Cancelled' }];

        restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        restService.Get('ReferencelistService/PERSONS', true).then(function (results) {
            $scope.Persons = results;
        });


        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
        });

        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Company.push(v);
            });
            //$scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
            //$scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;

        });

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Location.push(v);
            })
        });

        restService.Get('ReferencelistService/TRANSPORTWITHCHARGE', true).then(function (results) {
            $scope.Transports = results;
            $scope.PreLoad();
        });

        $scope.gettransportname = function (id) {
            try {
                var found = $scope.Transports.find(function (arritem) {
                    return arritem.id == id;
                });
                if (typeof (found) != 'undefined' && found != null)
                    return found.name;
            } catch (e) {
                //alert(e);
            }
        }

        $scope.typechanged = function () {
            if ($scope.SearchRequest.typeid == 6 || $scope.SearchRequest.typeid == 7 || $scope.SearchRequest.typeid == 0) {
                $scope.filteredtype = '';
            }
            else {
                var ftype = $scope.ItemTypes.find(function (item) {
                    return $scope.SearchRequest.typeid == item.id;
                });
                if (ftype != null && ftype.id > 0)
                    $scope.filteredtype = ftype.name;
                else
                    $scope.filteredtype = '';
            }
        };


        $scope.supplierchanged = function () {
            var ftype = $scope.Suppliers.find(function (item) {
                return $scope.SearchRequest.supplierid == item.id;
            });
            if (ftype != null && ftype.id > 0)
                $scope.filteredtype = ftype.name;
            else
                $scope.filteredtype = '';
        };

        $scope.statuschanged = function (status) {
            var rows = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                v.status = status;
                rows.push(v);
            });
            request = {
                "Items": rows,
                "SecurityToken": "String content"
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillinboxService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $scope.Search();
            });
        };

        $scope.SearchRequest = {
            typeid: -1,
            addedfrom: null,
            supplierid: null,
            status: -1,
            addedto: null
        };

        restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Suppliers.push(v);
            })
        });

        restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.City.push(v);
            })
        });

        $scope.Search = function () {
            $scope.LRInboxList = [];
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillinboxService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.igrid == null) {
                    $scope.ConfigGrid();
                }
                $scope.LRInboxList = result;
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        }


        $scope.mergePDF = function () {
            var ids = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                ids.push(v.billid);
            });
            var request = {
                "Item": { 'ids': ids },
                "SecurityToken": "String content"
            };

            $rootScope.PostDownload("BillService/MergePrintPDF", request, 'billpdf.pdf', 'application/pdf');

        }

        $scope.PrintUnbackedBill = function (item) {
            $rootScope.Download("BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.billid + "/false", 'Invoice No -' + item.billno + '.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill_" + item.billid + ".pdf";
                link.href = "BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.billid + "/false";
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.PrintBill = function (item) {
            $rootScope.Download("BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.billid + "/false", 'Invoice No -' + item.billno + '.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill_" + item.billid + ".pdf";
                link.href = "BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.billid + "/false";
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.PrintDeliverySlip = function (item) {
            $rootScope.Download("BillService/PrintDeliverySlip/" + $scope.UserInfo.OrganizationID + '/' + item.billid, "Invoice No -" + item.billno + ".pdf", 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill_" + item.billid + ".pdf";
                link.href = "BillService/PrintDeliverySlip/" + $scope.UserInfo.OrganizationID + '/' + item.billid;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.CancelComformation = function (item, isremove) {
            $.confirm({
                title: 'Cancel',
                content: 'Do you want to cancel the selected bill, this action cannot be revoked.',
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
                            $scope.Cancel(item, isremove);
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

        $scope.Cancel = function (item, isremove) {

            if (item.settled) {
                $rootScope.Toaster.Notice("Settled bill can't cancel, Bill No - " + item.billno);
                return;
            }
            request = {
                "item": {
                    'ids': [item.billid], 'locationid': $rootScope.LocationInfo.location.id,
                    'isbulkreturn': false,
                    'disablestockreverse': isremove,
                    'isbulksale': false
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Cancellation",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $rootScope.Toaster.Notice('Sales Invoice Cancelled Successfully, Bill No - ' + item.billno);
                    request = {
                        "Items": [{ 'id': item.id, 'status': -9 }],
                        "SecurityToken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "BillinboxService/UpdateStatus",
                        data: srequest
                    }).success(function (result) {
                        $scope.Search();
                    }).error(function (result) {
                    });
                }

            }).error(function (result) {
            });
        };



        $scope.ManageLR = function (item, callback) {
            var dialog = ngDialog.open({
                template: 'ManageLR',
                data: [$scope, item],
                width: '850px',
                controller: ['$scope', '$timeout', '$http', 'restService', '$rootScope', function ($scope, $timeout, $http, restService, $rootScope) {
                    $scope.entity = $scope.ngDialogData[1];
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                    $scope.editeinvoice = false;
                    $scope.editeway = false;
                    if (typeof ($scope.entity.billimg) != 'undefined' &&
                        $scope.entity.billimg != null && $scope.entity.billimg != '') {
                        $scope.imagelist = $scope.entity.billimg.split(',');
                    }
                    else {
                        $scope.imagelist = [];
                    }
                    if ($scope.entity.lrdate == 'undefined' || $scope.entity.lrdate == 'null'
                        || $scope.entity.lrdate == '0001-01-01T00:00:00') {
                        $scope.entity.lrdate = new Date();
                    }

                    $scope.publish = function () {
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/Publish",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog();
                        });
                    }

                    $scope.TransportChanged = function () {
                        var transport = $scope.Parent.Transports.find(function (item) {
                            return item.id == $scope.entity.transportid;
                        });
                        if (transport && transport != null) {
                            $scope.entity.vehicleno = transport.vehicles.split(',')[0];
                        }
                    }

                    $scope.billimg = null;
                    $scope.GenerateEInvoice = function (mode) {
                        $scope.entity.transportname = $scope.Parent.gettransportname($scope.entity.transportid);
                        $scope.entity.mode = mode
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/GenerateEInvoice",
                            data: srequest
                        }).success(function (result) {

                            if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                var errors = [];
                                $.each(result.ErrorDetails, function (k, v) {
                                    errors.push(v.ErrorMessage);
                                });
                                $rootScope.ShowAlert('Error', errors.join('<br>'));
                            } else {
                                $scope.entity.version = result.BillInbox.version;
                                $scope.entity.einvoice = result.BillInbox.einvoice;
                                $scope.entity.ewb = result.BillInbox.ewb;
                                $scope.entity.modifiedby = result.BillInbox.modifiedby;
                                $scope.entity.modifiedon = result.BillInbox.modifiedon;
                            }

                        });
                    }

                    $scope.GenerateEWay = function (mode) {
                        $scope.entity.transportname = $scope.Parent.gettransportname($scope.entity.transportid);
                        $scope.entity.mode = mode
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/GenerateEWay",
                            data: srequest
                        }).success(function (result) {

                            if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                var errors = [];
                                $.each(result.ErrorDetails, function (k, v) {
                                    errors.push(v.ErrorMessage);
                                });
                                $rootScope.ShowAlert('Error', errors.join('<br>'));
                            } else {
                                $scope.entity.version = result.BillInbox.version;
                                $scope.entity.ewb = result.BillInbox.ewb;
                                $scope.entity.modifiedby = result.BillInbox.modifiedby;
                                $scope.entity.modifiedon = result.BillInbox.modifiedon;
                            }

                        });
                    }


                    $scope.CancelEway = function (mode) {
                        $scope.Question('Eway Cancellation', 'Are you sure, do you want to cancel the eway for this bill', function () {
                            $scope.InternalCancelEway(mode);
                        })
                    }

                    $scope.InternalCancelEway = function (mode) {
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/CancelEway",
                            data: srequest
                        }).success(function (result) {

                            if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                var errors = [];
                                $.each(result.ErrorDetails, function (k, v) {
                                    errors.push(v.ErrorMessage);
                                });
                                $rootScope.ShowAlert('Error', errors.join('<br>'));
                            } else {
                                $scope.entity.version = result.BillInbox.version;
                                $scope.entity.einvoice = result.BillInbox.einvoice;
                                $scope.entity.ewb = result.BillInbox.ewb;
                                $scope.entity.modifiedby = result.BillInbox.modifiedby;
                                $scope.entity.modifiedon = result.BillInbox.modifiedon;
                            }

                        });
                    }

                    $scope.Question = function (title, content, callback) {
                        $.confirm({
                            title: title,
                            content: content,
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
                                        callback();
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

                    $scope.CancelEInvoice = function (mode) {
                        $scope.Question('Einvoice Cancellation', 'Are you sure, do you want to cancel the e-invoice for this bill', function () {
                            $scope.InternalCancelEInvoice(mode);
                        })
                    }


                    $scope.InternalCancelEInvoice = function (mode) {
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/CancelEInvoice",
                            data: srequest
                        }).success(function (result) {

                            if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                var errors = [];
                                $.each(result.ErrorDetails, function (k, v) {
                                    errors.push(v.ErrorMessage);
                                });
                                $rootScope.ShowAlert('Error', errors.join('<br>'));
                            } else {
                                $scope.entity.version = result.BillInbox.version;
                                $scope.entity.einvoice = result.BillInbox.einvoice;
                                $scope.entity.ewb = result.BillInbox.ewb;
                                $scope.entity.modifiedby = result.BillInbox.modifiedby;
                                $scope.entity.modifiedon = result.BillInbox.modifiedon;
                            }

                        });
                    }

                    $scope.BillImageChanged = function (imageid) {
                        if (typeof (imageid) != 'undefined') {
                            if (angular.isArray(imageid)) {
                                if (imageid[0] != null && imageid[0] != '') {
                                    $scope.imagelist.splice(0, 0, imageid[0]);
                                }
                            }
                            else if (imageid != null && imageid != '') {
                                $scope.imagelist.splice(0, 0, imageid);
                            }
                        }
                        else {
                            $scope.imagelist.splice(0, 0, $scope.billimg);
                        }
                        $scope.billimg = null;
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }

                    $scope.RemoveImage = function (item) {
                        $scope.imagelist.splice($scope.imagelist.indexOf(item), 1);
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }


                    $scope.Save = function () {
                        $scope.entity.modeofdelivery = 1;
                        $scope.entity.transportname = $scope.Parent.gettransportname($scope.entity.transportid);
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        callback(data.value);
                    }
                }
            });

        };

        $scope.PrintBarcode = function () {
            var billid = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                billid.push(v.billid);
            });
            var request = {
                "Item": { 'billids': billid },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StockService/BillBarcodeList",
                data: srequest
            }).success(function (result) {
                $scope.ChooseBarcode(result);
            });
        }

        $scope.ChooseBarcode = function (barcodeList) {
            $.each(barcodeList, function (k, v) {
                v.sno = k + 1;
            });
            var dialog = ngDialog.open({
                template: 'ChooseBarecodes',
                data: {
                    'restService': $scope.restService,
                    'currentlist': barcodeList
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.restService = $scope.ngDialogData.restService;
                    $scope.currentlist = $scope.ngDialogData.currentlist;
                    $scope.SelectAll = function (value) {
                        $.each($scope.currentlist, function (k, v) {
                            v.selected = value;
                        })
                    }

                    $scope.Selected = function () {
                        var returnList = []
                        $.each($scope.currentlist, function (k, v) {
                            if (v.selected) returnList.push(v);
                        });
                        $scope.closeThisDialog(returnList);
                    }

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        if (data.value.length > 0) {

                            var requestitem = { 'barcodes': data.value };
                            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                                    return;

                                requestitem.reportid = printdata.report.id;

                                var request = {
                                    "Item": requestitem,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "StockService/PrintBillBarcode",
                                    data: srequest
                                }).success(function (result) {
                                    if (result != null && result.length > 0) {
                                        $rootScope.Sent2Printer(result.join(';'), printdata);
                                    }
                                });
                            });

                        }
                    }
                }
            });
        }



        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.GetSelecteItems = function (type, statusid, modeofdelivery) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v.referenceid);
            });
            return retList;
        };

        $scope.GetSelecteItemsstatus = function () {

            var seletedRows = this.igrid.GetSelectedRows();

            var retList = [];
            $.each(seletedRows, function (k, v) {
                retList.push(v);
            });
            return retList;
        };

        $scope.PreLoad = function () {
            var today = new Date();
            var fromday = new Date();
            fromday.setDate(today.getDate() - 10);

            $scope.SearchRequest.addedfrom = $filter('date')(fromday, datePicker.shortDisplay);
            $scope.SearchRequest.addedto = $filter('date')(today, datePicker.shortDisplay);
            request = {
                "Item": {
                    typeid: -1,
                    addedfrom: $filter('date')(fromday, datePicker.shortDisplay),
                    addedto: $filter('date')(today, datePicker.shortDisplay),
                    showallopen: true,
                    status: -1,
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillinboxService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.igrid == null) {
                    $scope.ConfigGrid();
                }
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'B2B SALES INVOICE - INFORMATION'
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


        $scope.StatusChange = function (action) {

            var items = $scope.GetSelecteItemsstatus();
            if (items.length == 0)
                return;

            var inboxitems = [];
            $.each(items, function (k, v) {
                var inboxstatus = { 'id': v.id, 'modeofdelivery': 0 };
                if (action == "Hand Delivery") {
                    inboxstatus.statusid = 0;
                }
                else if (action == "Received") {
                    inboxstatus.statusid = 1;
                }
                else if (action == "Rejected") {
                    inboxstatus.statusid = 2;
                }
                inboxitems.push(inboxstatus);
            });

            request = {
                "Items": inboxitems,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillinboxService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $.each(items, function (k, v) {
                    $scope.LRInboxList.splice($scope.LRInboxList.indexOf(v), 1);
                });
            }).error(function (result) {
            });
        };

        $scope.ChangeBillNo = function (item) {
            $rootScope.Prompt('Change Bill No from ' + item.billno, 'New Bill No', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {

                    var request = {
                        "Item": {
                            'bill': item,
                            'newbillno': code
                        },
                        "SecurityToken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "BillinboxService/ChangeBillNo",
                        data: srequest
                    }).success(function (result) {
                        $rootScope.Toaster.Notice(result);
                        $scope.Search();
                    }).error(function (result) {
                    });
                }
            }, item.billno);
        }

        $scope.EditBill = function (billitem, callback) {
            var dialog = ngDialog.open({
                template: 'EditBill',
                data: [$scope, billitem],
                width: '80%',
                controller: ['$scope', '$timeout', '$http', 'restService', '$timeout', 'Excel',
                    function ($scope, $timeout, $http, restService, $timeout, Excel) {
                        $scope.billitem = $scope.ngDialogData[1];
                        $scope.CurrentBill = null;
                        $scope.Customers = $scope.ngDialogData[0].Customers;
                        $scope.MarginChanged = function (item) {
                            if (typeof (item.barcodeDetail) == 'undefined')
                                item.barcodeDetail = {
                                    'taxid': item.taxid,
                                    'discount': item.qty > 0 ? (item.discount / item.qty) : 0,
                                    'additionaldiscount': item.additionaldiscount,
                                    'stockid': item.stockid,
                                    'id': item.salablegoodsid,
                                    'companyid': item.companyid
                                };

                            item.cost = $scope.GetNumber(item.attributes.cost);
                            item.amount = item.cost * $scope.GetNumber(item.qty);
                            if ($scope.GetNumber(item.attributes.margin) > 0) {
                                var currentcomputedprice = (item.cost + (item.cost * $scope.GetNumber(item.attributes.margin) / 100)).round(0);
                                var currentprice = item.price.round(0);
                                if (currentcomputedprice == currentprice) {
                                    var taxpart = item.price / (100 + item.taxpercentage) * item.taxpercentage;
                                    var newprice = item.price - taxpart;
                                    var newmargin = ((newprice / item.cost) * 100) - 100;
                                    item.attributes.margin = newmargin;
                                }
                                item.price = (item.cost + (item.cost * $scope.GetNumber(item.attributes.margin) / 100)).round(2)
                            }
                            else {
                                item.price = item.cost;
                            }

                            if ($scope.GetNumber(item.additionaldiscountpercentage) > 0) {
                                item.additionaldiscount = (item.price * $scope.GetNumber(item.additionaldiscountpercentage) / 100).round(2)
                                item.price -= item.additionaldiscount;
                            }

                            item.price = Math.round(item.price);

                            item.rate = item.price;
                            item.salerate = item.price;

                            if (!$scope.CurrentBill.isinterstatesale) {
                                var halfTax = (item.price * (item.taxpercentage / 2) / 100).round(5);
                                item.itemtax = (halfTax * 2).round(5);
                            }
                            else {
                                item.itemtax = (item.price * item.taxpercentage / 100).round(5);
                            }

                            item.price = (item.price + item.itemtax).round(0);
                            item.tax = (item.itemtax * item.qty).round(5);
                            item.amount = ((item.salerate * item.qty) + item.tax).round(0);

                            item.attributes.gross = item.cost + (item.cost * item.attributes.margin / 100).round(0);
                            item.etprice = item.attributes.gross;
                            item.discount = (item.qty * (item.barcodeDetail.discount + item.additionaldiscount)).round(2);

                            if (typeof (item.additionaldiscount) == 'undefined' || isNaN(item.additionaldiscount)) {
                                item.additionaldiscount = 0;
                                item.additionaldiscountpercentage = 0;
                            }

                            if (typeof (item.schemediscount) == 'undefined' || isNaN(item.schemediscount)) {
                                item.schemediscount = 0;
                                item.schemediscountpercentage = 0;
                            }

                            item.receivable = (item.salerate * item.qty + item.tax).round(2);
                            $scope.ComputeTotal();
                        }

                        $scope.ApplyDiscount = function () {
                            var discountpercentage = parseFloat($scope.CurrentBill.newadditionaldiscount);
                            if (isNaN(discountpercentage))
                                discountpercentage = 0;
                            $.each($scope.CurrentBill.Billitems, function (k, v) {
                                v.additionaldiscountpercentage = discountpercentage;
                                $scope.MarginChanged(v);
                            })
                            $scope.ComputeTotal();
                        }

                        $scope.DownloadExcel = function () {
                            $scope.exportHref = Excel.tableToExcel('billdetails', 'B2B Bill', 1, 'Bill Items');
                            if ($scope.exportHref != null) {
                                $timeout(function () {
                                    var link = document.createElement('a');
                                    link.download = "b2binvoice.xls";
                                    link.href = $scope.exportHref;
                                    document.body.appendChild(link);
                                    link.click();
                                    link.remove();
                                }, 100);
                            }
                        }

                        $scope.ComputeTotal = function () {
                            var total = 0;
                            var tax = 0;
                            $.each($scope.CurrentBill.Billitems, function (k, v) {
                                total += v.receivable;
                                tax += v.tax;
                            });
                            $scope.CurrentBill.receivable = total;
                            $scope.CurrentBill.tax = tax;
                            $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
                            $scope.CurrentBill.total = $scope.CurrentBill.receivable + $scope.CurrentBill.rounding;
                        }

                        $scope.Deleteitem = function (item) {
                            $scope.CurrentBill.Billitems.splice($scope.CurrentBill.Billitems.indexOf(item), 1);
                            $scope.ComputeTotal();
                        }

                        $scope.SaveRequest = false;
                        $scope.Save = function () {
                            if ($scope.SaveRequest) return;
                            if (!$scope.CurrentBill.isb2b && $scope.CurrentBill.iscreditbill) {
                                $scope.CurrentBill.isb2b = true;
                            } else if ($scope.CurrentBill.isb2b && !$scope.CurrentBill.iscreditbill) {
                                $scope.CurrentBill.isb2b = false;
                            }
                            var request = {
                                "Item": $scope.CurrentBill
                            }
                            $scope.SaveRequest = true;
                            $http({
                                method: 'POST',
                                url: "BillService/Save",
                                data: JSON.stringify(request)
                            }).success(function (result) {
                                $scope.SaveRequest = false;
                                $scope.closeThisDialog($scope.CurrentBill);
                            });
                        }

                        $scope.LoadDetails = function () {
                            request = {
                                "Item": {
                                    'id': $scope.billitem.billid
                                },
                                "SecurityToken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "BillService/Search",
                                data: srequest
                            }).success(function (result) {
                                $scope.CurrentBill = result[0];
                                $.each($scope.CurrentBill.Billitems, function (k, v) {
                                    if (typeof (v.attributes) == 'undefined') v.attributes = {};
                                    if (typeof (v.attributes.cost) == 'undefined') v.attributes.cost = v.rate;
                                    if (typeof (v.attributes.margin) == 'undefined') v.attributes.margin = 0;
                                    if (typeof (v.attributes.gross) == 'undefined') v.attributes.gross = item.amount;
                                    v.cost = v.attributes.cost;
                                    v.buyingprice = v.attributes.cost;
                                });
                            });
                        }

                        $timeout(function () {
                            $scope.setFocuscloseit();
                            $scope.LoadDetails();
                        }, 100);

                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        callback(data.value);
                    }
                }
            });
        }

        $scope.BillAttributeChange = function (billitem, callback) {

            var items = $scope.igrid.GetSelectedRows();
            if (items.length != 1)
                return;

            var dialog = ngDialog.open({
                template: 'BillAttributeChange',
                data: [$scope, items[0]],
                width: '450px',
                controller: ['$scope', '$timeout', '$http', 'restService', '$timeout', 'Excel',
                    function ($scope, $timeout, $http, restService, $timeout, Excel) {
                        $scope.billitem = $scope.ngDialogData[1];
                        $scope.CurrentBill = null;
                        $scope.Customers = $scope.ngDialogData[0].Customers;

                        $scope.SaveRequest = false;
                        $scope.Save = function () {
                            if ($scope.SaveRequest) return;
                            if (!$scope.CurrentBill.isb2b && $scope.CurrentBill.iscreditbill) {
                                $scope.CurrentBill.isb2b = true;
                            } else if ($scope.CurrentBill.isb2b && !$scope.CurrentBill.iscreditbill) {
                                $scope.CurrentBill.isb2b = false;
                            }
                            var request = {
                                "Item": $scope.CurrentBill
                            }
                            $scope.SaveRequest = true;
                            $http({
                                method: 'POST',
                                url: "BillService/UpdateAttributes",
                                data: JSON.stringify(request)
                            }).success(function (result) {
                                $scope.SaveRequest = false;
                                $scope.closeThisDialog($scope.CurrentBill);
                            });
                        }

                        $scope.LoadDetails = function () {
                            request = {
                                "Item": {
                                    'id': $scope.billitem.billid
                                },
                                "SecurityToken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "BillService/Search",
                                data: srequest
                            }).success(function (result) {
                                $scope.CurrentBill = result[0];
                            });
                        }

                        $timeout(function () {
                            $scope.setFocuscloseit();
                            $scope.LoadDetails();
                        }, 100);

                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        callback(data.value);
                    }
                }
            });
        }

        $scope.DownloadPDF = function () {
            var items = $scope.igrid.GetSelectedRows();
            if (items.length != 1)
                return;
            var dialog = ngDialog.open({
                template: 'ChooseReportFormat',
                data: {
                    'scope': $scope
                },
                width: '300px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.ReportList = null;
                    $scope.ParentScope = $scope.ngDialogData.scope;
                    restService.Get('ConfigurationService/ReportList?id=' + 'SALERETURNINVOICE,SALEINVOICE', true).then(function (results) {
                        $scope.ReportList = results;
                    });
                    $scope.Selected = function () {
                        if ($scope.printingformatid == undefined ||
                            $scope.printingformatid == null) {
                            $scope.ParentScope.Toaster.Info('Please select default printing format');
                            return;
                        }
                        $scope.closeThisDialog({
                            'printingtemplateid': $scope.printingformatid
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $rootScope.Download("BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + items[0].billid + "/false/" + data.value.printingtemplateid, 'Invoice No -' + items[0].billno + '.pdf', 'application/pdf');
                    }
                }
            });
        }


        $scope.addbill2inbox = function () {
            $rootScope.Prompt('Add 2 Inbox', 'Bill No', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {
                    var request = {
                        "Item": {
                            'billno': code
                        },
                        "SecurityToken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "BillinboxService/Save",
                        data: srequest
                    }).success(function (result) {
                        if (result.id > 0) {
                            $scope.LRInboxList.push(result);
                            $scope.igrid.UpdateData($scope.LRInboxList);
                        }
                    });
                }
            })
        }

        $scope.ChangeStatus = function () {
            var items = $scope.igrid.GetSelectedRows();
            if (items.length == 0)
                return;

            var dialog = ngDialog.open({
                template: 'ChooseStatus',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.StatusList = [
                        { 'id': 4, 'name': 'Packed' },
                        { 'id': 5, 'name': 'On Transit' },
                        { 'id': 6, 'name': 'At Destination' },
                        { 'id': 7, 'name': 'Reached' },
                        { 'id': 8, 'name': 'On Return' },
                        { 'id': 9, 'name': 'Returned' }
                    ];

                    $scope.Selected = function () {
                        if ($scope.statusid && $scope.statusid != null &&
                            $scope.statusid != '') {
                            $scope.closeThisDialog($scope.statusid);
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {
                        $scope.statuschanged(data.value);
                    }
                }
            });
        }

        $scope.CreatePerson = function (callback) {
            var dialog = ngDialog.open({
                template: 'ManagePersons',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {

                    $scope.PersonRoles = [
                        { 'id': 1, 'name': 'Lorry Pickup' },
                        { 'id': 2, 'name': 'Courier Pickup' },
                        { 'id': 3, 'name': 'General' },
                    ];

                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'type': 'PERSONS', 'code': $scope.code, 'name': $scope.name, 'referenceid': $scope.referenceid });
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
                            url: "ReferencelistService/ManageAttribute",
                            data: srequest
                        }).success(function (result) {
                            $scope.Persons.push(result);
                            $scope.Persons = $scope.Persons.slice();
                        }).error(function (result) {
                        });

                    }
                }
            });
        }
        $scope.igriddelivery = null;
        $scope.ShowDelivery = function () {
            $scope.EnableDelivery = true;
            $scope.SearchRequest.statusid = 10;
            if ($scope.HandOverList == undefined || $scope.HandOverList == null || $scope.HandOverList.length == 0) {
                $scope.HandOverList = [];
                if (this.igrid && this.igrid != null) {
                    $.each(this.igrid.GetSelectedRows(), function (k, v) {
                        if (v.handedtoid == undefined || v.handedtoid == null || v.handedtoid == 0)
                            $scope.HandOverList.push(v);
                    });
                }
            }
            if ($scope.igriddelivery != null) {
                $scope.igriddelivery.UpdateData($scope.HandOverList);
                return;
            }
            $timeout(function () {
                var igrid = new InfyGrid('gridBillListContainer', '425px', '100%', true, true, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.AddButtonColumn('Remove', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Remove' title='Remove' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.AddSimpleColumn('customername', 'Customer Name', 150);
                igrid.AddReferenceColumn('billshipto', 'Ship To', 140, $scope.Customers);
                igrid.AddSimpleColumn('billno', 'Invoice No', 100);
                igrid.AddFormatedColumn('noofbundles', 'No of Bundle', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('scanned', 'Scanned', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('billdate', 'Invoice Date', 80, 'dd-MM-yyyy');
                //igrid.AddFormatedColumn('billvalue', 'Invoice Amout', 80, 'N2', 2).total = true;
                //igrid.AddFormatedColumn('billqty', 'Invoice Qty', 80, 'N2', 2).total = true;
                igrid.AddSimpleColumn('lrno', 'LR No', 100);
                igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd-MM-yyyy');
                igrid.AddReferenceColumn('tolocationid', 'Delivery City', 100, $scope.City);
                igrid.AddReferenceColumn('transportid', 'Transport', 140, $scope.Transports);

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igriddelivery = igrid;

                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command == 'Remove') {
                        $timeout(function () {
                            $scope.RemoveHandover(item);
                        }, 10);
                    }
                });

                igrid.dataView.getItemMetadata = function (row) {
                    var item = igrid.dataView.getItem(row);
                    if (item.scanned && item.scanned > 0) {
                        if (item.scanned == item.noofbundles) {
                            item.style = 'igridbigrowswithgreen';
                            return { cssClasses: 'igridbigrowswithgreen' };
                        }
                        else {
                            item.style = 'igridbigrowswithred';
                            return { cssClasses: 'igridbigrowswithred' };
                        }
                    }
                    else {
                        item.style = '';
                        return { cssClasses: '' };
                    }
                };

                $scope.igriddelivery.UpdateData($scope.HandOverList);
            }, 100);
        }

        $scope.RemoveHandover = function (item) {
            $scope.HandOverList.splice($scope.HandOverList.indexOf(item), 1);
            $scope.igriddelivery.UpdateData($scope.HandOverList);
        }

        $scope.SaveBill4Handover = function () {
            var existinghanded = $scope.HandOverList.find(function (item) { return item.handedtoid > 0; });
            var handedoveron = existinghanded && existinghanded != null ? existinghanded.handedoveron : new Date();

            var handOverList = (JSON.parse(JSON.stringify($scope.HandOverList)));
            var messages = [];
            $.each(handOverList, function (k, v) {
                v.handedoveron = handedoveron;
                v.handedtoid = $scope.SearchRequest.handedtoid;
                //v.transportid = $scope.SearchRequest.transportid;
                if (v.scanned != v.noofbundles) {
                    messages.push(v.billno + ' Scanned ' + v.scanned + ' Expected ' + v.noofbundles);
                }
            });

            if (messages.length > 0) {
                $rootScope.Toaster.Error(messages.join('<br>'));
                return;
            }

            request = {
                "Items": handOverList,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillinboxService/SaveHandover",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $scope.HandOverList = handOverList;
                    $scope.igriddelivery.UpdateData($scope.HandOverList);
                }
            });
        }

        $scope.PrintHandover = function () {
            var nonsaved = $scope.HandOverList.find(function (item) {
                return item.handedtoid == undefined || item.handedtoid == null || item.handedtoid == 0;
            });
            if (nonsaved && nonsaved != null) {
                $rootScope.Toaster.Notice("Please save before print");
                return;
            }

            $rootScope.PostDownload("BillinboxService/PrintPDF",
                { 'Items': $scope.HandOverList }, 'deliverypersonslip.pdf', 'application/pdf');
        }

        $scope.CreateHandover = function () {
            $scope.CreateDelivery = true;
            if ($scope.SearchRequest == undefined)
                $scope.SearchRequest = {};
            $scope.SearchRequest.handeddate = new Date();
            $scope.HandOverList = [];
            if ($scope.igriddelivery == null) {
                $scope.ShowDelivery();
            }
            $scope.igriddelivery.UpdateData($scope.HandOverList);
        }

        $scope.Search4HandOver = function () {
            if ($scope.gridBillhandedContainer && $scope.gridBillhandedContainer != null)
                $scope.gridBillhandedContainer.destroy();

            var request = {
                "Item": {
                    'transportid': $scope.SearchRequest.transportid,
                    'handedtoid': $scope.SearchRequest.handedtoid,
                    'addedfrom': $scope.SearchRequest.addedfrom,
                    'addedto': $scope.SearchRequest.addedto,
                    'status': 10
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillinboxService/Search",
                data: srequest
            }).success(function (result) {
                var igrid = new InfyGrid('gridBillhandedContainer', '425px', '100%', true, true, true, 150);
                igrid.options.showHeaderRow = true;
                igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' title='Edit' class='btn btn-success btn-xs'><i class='fa fa-file' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.AddSimpleColumn('transportname', 'Transport', 100);
                igrid.AddSimpleColumn('handoverperson', 'Handedover', 100);
                igrid.AddFormatedColumn('handoveron', 'Handed On', 80, 'dd-MM-yyyy');
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command == 'Edit') {
                        $timeout(function () {
                            if ($scope.igriddelivery && $scope.igriddelivery != null) {
                                $scope.igriddelivery.destroy();
                            }
                            $scope.SearchRequest.transportid = item.transportid;
                            $scope.SearchRequest.handedtoid = item.handoverto;
                            $scope.SearchRequest.handeddate = item.handoveron;
                            $scope.HandOverList = item.items;
                            $scope.igriddelivery = null;
                            $scope.CreateDelivery = true;
                            $scope.ShowDelivery();
                        }, 50);
                    }
                });
                $scope.ShowHandoverReport(result);
                igrid.UpdateData(result);
                $scope.gridBillhandedContainer = igrid;
            }).error(function (result) {
            });
        }

        $scope.ShowHandoverReport = function (result) {
            if ($scope.igriddelivery && $scope.igriddelivery != null) {
                $scope.igriddelivery.destroy();
            }
            $scope.HandOverList = null;
            var items = [];
            $.each(result, function (k, v) {
                $.each(v.items, function (kk, vv) {
                    items.push(vv);
                });
            })
            var igrid = new InfyGrid('gridBillListContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('customername', 'Customer Name', 150);
            igrid.AddReferenceColumn('billshipto', 'Ship To', 140, $scope.Customers);
            igrid.AddSimpleColumn('billno', 'Invoice No', 100);
            igrid.AddFormatedColumn('billdate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('tolocationid', 'Delivery City', 100, $scope.City);
            igrid.AddReferenceColumn('transportid', 'Transport', 140, $scope.Transports);
            igrid.AddFormatedColumn('noofbundles', 'No of Bundle', 80, 'N2', 0).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igriddelivery = igrid;
            $scope.igriddelivery.UpdateData(items);
        }

        $scope.HandOverList = [];
        $scope.SearchBill4Handover = function () {

            //if ($scope.SearchRequest == undefined ||
            //    $scope.SearchRequest.transportid == undefined ||
            //    $scope.SearchRequest.transportid == null ||
            //    $scope.SearchRequest.transportid == 0) {
            //    $rootScope.Toaster.Notice("Please select the transport");
            //    return;
            //}

            if ($scope.SearchRequest == undefined ||
                $scope.SearchRequest.handedtoid == undefined ||
                $scope.SearchRequest.handedtoid == null ||
                $scope.SearchRequest.handedtoid == 0) {
                $rootScope.Toaster.Notice("Please select the handover person");
                return;
            }

            if ($scope.SearchRequest == undefined ||
                $scope.SearchRequest.handeddate == undefined ||
                $scope.SearchRequest.handeddate == null ||
                $scope.SearchRequest.handeddate == '') {
                $rootScope.Toaster.Notice("Please select the handover date");
                return;
            }

            var exist = $scope.HandOverList.find(function (item) {
                return item.billno == $scope.handover.billno;
            });
            if (exist && exist != null) {
                if (exist.scanned == undefined || exist.scanned == null)
                    exist.scanned = 0;
                exist.scanned++;
                $rootScope.Toaster.Notice("Added - " + exist.scanned);
                $scope.igriddelivery.UpdateData($scope.HandOverList);
                $scope.handover.billno = '';
                return;
            }

            request = {
                "Item": {
                    'billno': $scope.handover.billno,
                    'status': -1,
                    'typeid': -1
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillinboxService/Search",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 1) {

                    //if (result[0].transportid > 0 &&
                    //    result[0].transportid != $scope.SearchRequest.transportid) {
                    //    $rootScope.Toaster.Error("Only selected transport allowed, not allowed " + result[0].transportname);
                    //    return;
                    //}

                    $scope.handover.billno = '';
                    if (result[0].handedtoid && result[0].handedtoid > 0) {

                        $.confirm({
                            title: 'Change',
                            content: "It's already delivered, do you want to remove from previous delivery?",
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
                                        $scope.HandOverList.push(result[0]);
                                        $scope.igriddelivery.UpdateData($scope.HandOverList);
                                    }
                                },
                                cancel: function () {
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
                    else {
                        $scope.HandOverList.push(result[0]);
                        $scope.igriddelivery.UpdateData($scope.HandOverList);
                    }
                }
            }).error(function (result) {
            });
        }

        $scope.ConfigGrid = function () {

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.AddCustomColumn('haseinvoice', '', 30, function (item) {
                if (item.haseinvoice)
                    return "<img style='height:32px;width:32px;padding:4px 4px 4px 4px' src='clientside/dist/img/einvoice.png' />";
            });
            igrid.AddCustomColumn('haseway', '', 30, function (item) {
                if (item.haseway)
                    return "<img style='height:32px;width:32px;padding:4px 4px 4px 4px' src='clientside/dist/img/eway.png' />";
            });
            igrid.AddReferenceColumn('billlocationid', 'Bill Location', 100, $scope.Location);
            igrid.AddReferenceColumn('status', 'Status', 100, $scope.StatusList);
            igrid.AddSimpleColumn('customername', 'Customer Name', 150);

            if ($rootScope.UserInfo.appsettings.system_ismfgapplication) {
                igrid.AddSimpleColumn('billremarks', 'Bill Remark', 100);
                igrid.AddSimpleColumn('billsono', 'SO No', 100);
                igrid.AddReferenceColumn('billshipto', 'Ship To', 140, $scope.Customers);
            }
            if ($scope.ismulticompany)
                igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);

            igrid.AddSimpleColumn('billno', 'Invoice No', 100);
            igrid.AddFormatedColumn('billdate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('billvalue', 'Invoice Amout', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billqty', 'Invoice Qty', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('lrno', 'LR No', 100);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('tolocationid', 'Delivery City', 100, $scope.City);
            igrid.AddReferenceColumn('transportid', 'Transport', 140, $scope.Transports);
            igrid.AddFormatedColumn('noofbundles', 'No of Bundle', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('noofpieces', 'No of Pieces', 80, 'N2', 0).total = true;
            igrid.AddReferenceColumn('packedby', 'Packed By', 100, $scope.Employees);
            igrid.AddFormatedColumn('packedon', 'Delivered', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('ewb', 'Eway', 80);
            igrid.AddSimpleColumn('ackno', 'E-Ack', 80);
            igrid.AddButtonColumn('Print', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Print' value='Print' class='btn btn-primary btn-xs'><i class='fa fa-print' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Manage', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='LR Info' value='Manage' class='btn btn-default btn-xs'><i class='fa fa-tag' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Delivery Slip', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Delivery Slip' value='Delivery Slip' class='btn btn-info btn-xs'><i class='fa fa-paperclip' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            if ($scope.Permission.p_B2BSALES.delete) {
                var addremove = false || $scope.UserInfo.appsettings.system_supportbillnocharge;
                igrid.AddButtonColumn('Cancel', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Cancel' value='Cancel' class='btn btn-danger btn-xs'><i class='fa fa-scissors' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                if (addremove) {
                    igrid.AddButtonColumn('Remove', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Remove' title='Remove' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                    igrid.AddButtonColumn('Change No', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Change No' title='Change No' class='btn btn-danger btn-xs'><i class='fa fa-exchange' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                }
                $scope.UserInfo.system_supportbilledit = true;
                if ($scope.UserInfo.system_supportbilledit) {
                    igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Edit' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                }
            }

            //igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();

            $scope.igrid = igrid;
            //igrid.grid.setOptions({ 'frozenColumn': 6 });
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $scope.ApproveItem = null;
                $scope.SelectedCount = 0;
                $timeout(function () {
                    if (range.length == 1) {
                        var item = rowmodel.infyGrid.grid.getDataItem(range[0].fromRow);
                        if (item.status == 1)
                            $scope.ApproveItem = item;
                        else
                            $scope.SelectedCount = range.length;
                    }
                    else {
                        $scope.SelectedCount = range.length;
                    }
                }, 10);
            };
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Manage')
                    $scope.ManageLR(item, function (itm) {
                        item.version = itm.version;
                        $scope.igrid.referesh();
                    });
                if (command == 'Print')
                    $scope.PrintBill(item);
                if (command == 'Delivery Slip')
                    $scope.PrintDeliverySlip(item);
                if (command == 'Edit') {
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.EditBill(item, function (itm) {
                            itm.billvalue = itm.total;
                            $scope.igrid.referesh();
                        });
                    }
                }
                if (command == 'Cancel')
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.CancelComformation(item, false);
                    }
                if (command == 'Remove')
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.CancelComformation(item, true);
                    }
                if (command == 'Change No')
                    if (item.hascancel) {
                        $rootScope.Toaster.Notice("This bill already cancelled - " + item.billno);
                        return;
                    }
                    else {
                        $scope.ChangeBillNo(item);
                    }
            });
            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if (item.hascancel) {
                    item.style = 'igridbigrowswithred';
                    return { cssClasses: 'igridbigrowswithred' };
                }
                else {
                    if (item.status == 1) {
                        item.style = item.status == 1 ? 'igridbigrowswithmark' : '';
                        return { cssClasses: 'igriddebitnoterow' };
                    }
                    else {
                        item.style = item.packedby == 0 ||
                            item.transportid == 0 ||
                            item.lrno == '' ? 'igridwithgrey' : '';
                        return { cssClasses: '' };
                    }
                }
            };
            $scope.igrid.UpdateData([]);
        }


        $scope.uanpofileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            $scope.UANPoList = value;
            var dialog = ngDialog.open({
                template: 'ImportPO',
                data: {
                    'OrderInfo': value,
                    'ParentScope': $scope
                },
                width: '600px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.OrderInfo = $scope.ngDialogData.OrderInfo;
                    $scope.stockfailed = $scope.OrderInfo.ecomstatus.stocklist && $scope.OrderInfo.ecomstatus.stocklist.length > 0;
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Orders = $scope.OrderInfo.ecomstatus.orders;
                    $scope.ShowBillNo = $scope.OrderInfo.ecomstatus.ShowBillNo;
                    $scope.Count = 0;
                    $scope.Running = false;
                    $scope.SelectedAll = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            if (v.valid) {
                                v.selected = $scope.selectedall;
                                if ($scope.selectedall)
                                    $scope.Count += 1;
                            }
                        });
                    }
                    $scope.SelectedChanged = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            $scope.Count += (typeof (v.selected) != 'undefined' && v.selected) ? 1 : 0;
                        });
                    }

                    $scope.MissingStock = function () {
                        var data = [];
                        data.push([
                            'UAN',
                            'STOCK',
                            'SO',
                            'STO',
                            'POSSIBLE',
                            'REQUIRED'
                        ]);

                        var stock = 0;
                        var required = 0;
                        var so = 0;
                        var sto = 0;
                        var possible = 0;
                        $.each($scope.OrderInfo.ecomstatus.stocklist, function (k, v) {
                            data.push([
                                v.uan,
                                v.stock,
                                v.so,
                                v.sto,
                                (v.stock - (v.so + v.sto)),
                                v.required
                            ]);
                            stock += v.stock;
                            so += v.so;
                            possible += (v.stock - (v.so + v.sto));
                            sto += v.sto;
                            required += v.required;
                        });

                        data.push([
                            'Total',
                            stock,
                            so,
                            sto,
                            possible,
                            required,
                            '',
                        ]);

                        if (data.length > 0) {

                            var title = {									// set the style for target cell
                                font: {
                                    name: 'verdana',
                                    sz: 11,
                                    bold: true,
                                    color: { rgb: "33220000" }
                                },
                                alignment: {
                                    vertical: "center",
                                    horizontal: "center"
                                },
                                border: {
                                    top: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    bottom: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                }
                            };

                            var Stitle = {									// set the style for target cell
                                font: {
                                    name: 'verdana',
                                    sz: 11,
                                    bold: true,
                                    color: { rgb: "33220000" }
                                },
                                alignment: {
                                    vertical: "center",
                                    horizontal: "right"
                                },
                                border: {
                                    top: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    bottom: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                }
                            };
                            var boderstyle = {
                                border: {
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                },
                            }

                            var ws = XLSX.utils.aoa_to_sheet(data);
                            var wb = XLSX.utils.book_new();

                            ws["A1"].s = title;
                            ws["B1"].s = title;
                            ws["C1"].s = title;
                            ws["D1"].s = title;
                            ws["E1"].s = title;
                            ws["F1"].s = title;

                            ws["A" + data.length].s = Stitle;
                            ws["B" + data.length].s = Stitle;
                            ws["C" + data.length].s = Stitle;
                            ws["D" + data.length].s = Stitle;
                            ws["E" + data.length].s = Stitle;
                            ws["F" + data.length].s = Stitle;

                            ws['!cols'] = [{ width: 20 }, { width: 10 }, { width: 10 }, { width: 10 }, { width: 15 }, { width: 15 }];

                            var prefix = '';
                            var startfrom = 64;
                            for (var ridx = 1; ridx <= 6; ridx++) {
                                startfrom++;
                                for (var rnidx = 2; rnidx < data.length; rnidx++) {
                                    ws[prefix + String.fromCharCode(startfrom) + rnidx].s = boderstyle;
                                }
                                if (startfrom == 90) {
                                    charcode = 65;
                                    if (prefix != '') {
                                        var charcode = prefix.charCodeAt(prefix.length - 1);
                                        charcode++;
                                        if (charcode > 90) {
                                            charcode = 65;
                                        }
                                    }
                                    prefix = String.fromCharCode(charcode);
                                    startfrom = 64;
                                }
                            }

                            XLSX.utils.book_append_sheet(wb, ws, "Stock");
                            const wbout = XLSX.write(wb, { type: "array", bookType: "xlsx" });
                            saveAs(new Blob([wbout], { type: "application/octet-stream" }), "stockshortage.xlsx");
                        }
                    }

                    $scope.CheckStatus = function () {
                        restService.Get('File/ProcessStatus/' + $scope.UserInfo.OrganizationID + '/' + $scope.StatusID).then(function (result) {
                            $scope.UploadedingStatus = result;
                            $scope.ProgressValue = result.percentage;
                            if (!result.hascompleted) {
                                $scope.migrationtimer = $timeout(function () {
                                    $scope.CheckStatus();
                                }, 2000);
                            }
                            else {
                                var scount = 0;
                                var fcount = 0;
                                if (result.result && result.result != null) {
                                    $.each(result.result, function (k, v) {
                                        var ord = $scope.Orders.find(function (item) {
                                            return item.MOrderNo == v.sid;
                                        });
                                        if (v.id > 0) {
                                            scount++;
                                            if (ord && ord != null) {
                                                ord.errormessage = 'Imported';
                                            }
                                        }
                                        else {
                                            fcount++;
                                            if (ord && ord != null) {
                                                ord.errormessage = v.errormessage;
                                            }
                                        }
                                    })
                                    if (result.statuslist && result.statuslist != null && result.statuslist.length > 0) {
                                        $.alert("Total imported Orders - " + result.result.length + " Success - " + scount + " Failed - " + fcount + '<br>' +
                                            result.statuslist.join(' <br> '));
                                    }
                                    else {
                                        $.alert("Total imported Orders - " + result.result.length + " Success - " + scount + " Failed - " + fcount);
                                    }
                                }
                                else {
                                    $.alert("Completed");
                                }
                            }
                        });
                    }

                    $scope.Save = function () {
                        var selected = [];
                        var skipped = [];
                        $.each($scope.Orders, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected) {
                                if (v.valid) {
                                    selected.push(v);
                                }
                                else {
                                    skipped.push(v);
                                }
                            }
                        });
                        if (skipped.length > 0) {
                            $rootScope.Toaster.Info("Invalid orders can't import");
                        }
                        else {
                            $scope.Running = true;
                            $scope.OrderInfo.ecomstatus.orders = selected;
                            $scope.UploadedingStatus = { 'totalrows': selected.length, 'failedrows': 0, 'completedrows': 0, 'status': 'Starting' };
                            var request = {
                                "item": $scope.OrderInfo.ecomstatus,
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "BillinboxService/SaveUANECOM",
                                data: srequest
                            }).success(function (result) {
                                $scope.StatusID = result.identifier;
                                $scope.migrationtimer = $timeout(function () {
                                    $scope.CheckStatus();
                                }, 2000);
                            });
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                }
            });
        }


        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
                $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
                $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                $scope.IsUANBased = $rootScope.UserInfo.appsettings.mfg_mode == 5;
                $scope.Unicommerce = $rootScope.UserInfo.appsettings.int_unicommerce;
            } else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 10);
            }
        }

        $timeout(function () {
            $scope.LoadAppSetting();
        }, 10);
    }]);
