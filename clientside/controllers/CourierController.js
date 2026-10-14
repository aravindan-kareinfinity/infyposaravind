var app = angular.module('InfinityLab');

app.controller('CourierController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'Excel', 'ngDialog','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, ngDialog, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.Status = [];
        $scope.AllModes = [];
        $scope.InwardStatus = [{ 'id': 100, 'name': 'Received' },
        { 'id': 1, 'name': 'Working' },
            { 'id': 2, 'name': 'Completed' }];

        $scope.PayModes = [{ 'id': 1, 'name': 'Self' },
        { 'id': 2, 'name': 'To Pay' }];
        

        $scope.Modes = { 'Inward': 0, 'Outward': 1 };
        //$scope.Modes = [{ 'id': 0, 'name': 'Inward' }, { 'id': 1, 'name': 'Outward' }];

        $scope.OutwardStatus = [{ 'id': 102, 'name': 'Send' },
        { 'id': 103, 'name': 'Sent' }];

        $scope.ImageTypes = [{ 'id': 'Communication', 'name': 'Communication' },
        { 'id': 'Invoice', 'name': 'Invoice' },
        { 'id': 'LR', 'name': 'LR' },
        { 'id': 'EWay Bill', 'name': 'EWay Bill' },
        { 'id': 'Letter', 'name': 'Letter' },
        { 'id': 'Debit Note', 'name': 'Debit Note' },
        { 'id': 'Credit Note', 'name': 'Credit Note' }];

        

        $scope.CourierCompany = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.CourierCompany = results;
        });
        $scope.B2BCustomers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.B2BCustomers = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });

        $scope.SupplierInward = {
            "customer": false, "collectweight": false, "couriercompanyid": 0, "name": false, "weight": '', "inward": true,
            "payment": '', "billcopy": true, "detail": true, "agent": true, "customerb2b": false, "invoiceno": true,
            "rate": '', "collectpayment": false, "address": false, "supplier": true, "awbewb": false, "collectrate": false,
            "outward": false, "pod": true
        };
        $scope.SupplierOutward = {
            "customer": false, "collectweight": false, "couriercompanyid": 0, "name": false, "weight": '', "inward": false,
            "payment": '', "billcopy": false, "detail": true, "agent": true, "customerb2b": false, "invoiceno": true,
            "rate": '', "collectpayment": true, "address": false, "supplier": true, "awbewb": true, "collectrate": false,
            "outward": true, "pod": true
        };
        $scope.GeneralInward = {
            "customer": false, "collectweight": false, "couriercompanyid": 0, "name": true, "weight": '', "inward": true,
            "payment": '', "billcopy": false, "detail": true, "agent": false, "customerb2b": false, "invoiceno": false,
            "rate": '', "collectpayment": true, "address": true, "supplier": false, "awbewb": false, "collectrate": false,
            "outward": false, "pod": true
        };
        $scope.GeneralOutward = {
            "customer": false, "collectweight": false, "couriercompanyid": 0, "name": true, "weight": '', "inward": false,
            "payment": '', "billcopy": false, "detail": true, "agent": false, "customerb2b": false, "invoiceno": true,
            "rate": '', "collectpayment": true, "address": true, "supplier": false, "awbewb": true, "collectrate": false,
            "outward": true, "pod": true
        };
        $scope.CustomerOutward = {
            "customer": true, "collectweight": false, "couriercompanyid": 0, "name": false, "weight": '', "inward": false,
            "payment": '', "billcopy": false, "detail": true, "agent": false, "customerb2b": false, "invoiceno": true,
            "rate": '', "collectpayment": true, "address": false, "supplier": false, "awbewb": true, "collectrate": false,
            "outward": true, "pod": true
        };
        $scope.B2BCustomerOutward = {
            "customer": false, "collectweight": true, "couriercompanyid": 0, "name": false, "weight": '', "inward": false,
            "payment": '', "billcopy": false, "detail": true, "agent": false, "customerb2b": true, "invoiceno": true,
            "rate": '', "collectpayment": true, "address": false, "supplier": false, "awbewb": true, "collectrate": true,
            "outward": true, "pod": true
        };
        $scope.TypesofCourier = restService.Get('ReferencelistService/FULL-COURIERTYPE', true).then(function (results) {
            $scope.TypesofCourier = results;
            $scope.TypesofCourier.push({ id: -1, name: '- Supplier Inward -', 'additionalinfo': { 'courier': $scope.SupplierInward } });
            $scope.TypesofCourier.push({ id: -2, name: '- Supplier Outward -', 'additionalinfo': { 'courier': $scope.SupplierOutward } });
            $scope.TypesofCourier.push({ id: -3, name: '- General Inward -', 'additionalinfo': { 'courier': $scope.GeneralInward } });
            $scope.TypesofCourier.push({ id: -4, name: '- General Outward -', 'additionalinfo': { 'courier': $scope.GeneralOutward } });
            $scope.TypesofCourier.push({ id: -5, name: '- Customer Outward -', 'additionalinfo': { 'courier': $scope.CustomerOutward } });
            $scope.TypesofCourier.push({ id: -6, name: '- B2B Customer Outward -', 'additionalinfo': { 'courier': $scope.B2BCustomerOutward } });

            $scope.Courier.type = -1;
            $scope.ChangeConfiguration();
            $.each(results, function (k, v) {
                $scope.AllModes.push(v);
            });
            $.each(results, function (k, v) {
                $scope.AllModes.push(v);
            });
        });

        // $scope.Suppliers = [{ 'ID': 1, 'Name': 'Supplier1' }, { 'ID': 2, 'Name': 'Supplier2' }, { 'ID': 3, 'Name': 'Supplier3' }, { 'ID': 4, 'Name': 'Supplier4' }];

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            if ($location.search().id == undefined) {
                $rootScope.BindCompany($scope, 'Courier.receivingcompanyid');
            }
        });

        $scope.purchaseTaxActual = [];
        restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.purchaseTaxActual = results;
            var taxlist = [];
            $.each(results, function (k, v) {
                $.each(v.taxsplit.items, function (kk, vv) {
                    var taxitem = (JSON.parse(JSON.stringify(v)));
                    taxitem.taxsplit = null;
                    taxitem.taxpercentage = vv.cgst + vv.sgst;
                    if (v.type == 1 || v.type == 2) {
                        if (v.taxsplit.items.length > 1)
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '% (' + vv.fromamount + ' - ' + vv.toamount + ')';
                        else
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxitem.id = taxitem.id;
                        taxlist.push(taxitem);
                    }
                    else if (v.type == 4) {
                        taxitem.name = 'TDS @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxitem.id = taxitem.id;
                        taxlist.push(taxitem);
                    }
                });
            })
            $scope.purchaseTax = taxlist;
        });

        $scope.taxChanged = function () {
            if ($scope.Courier.taxid != null) {
                var taxitem = $scope.purchaseTax.find(function (item) {
                    return (item.id == $scope.Courier.taxid);
                });
                if (taxitem != null) {
                    $scope.Courier.taxpercentage = taxitem.taxpercentage;
                };
            };
        };


        // model to be used for Courier
        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "createdon": "",
            "modifiedon": "",
            "receivingcompanyid": 0,
            "companyid": 0,
            "receivedon": null,
            "senton": null,
            "type": 0,
            "islr": false,
            "podnumber": null,
            "awbnumber": null,
            "supplierid": 0,
            "status": 100,
            "receivedby": 0,
            "deliveredto": 0,
            "version": "1",
            "attachment": [],
            "agentid": 0,
            "isbillcopy": "",
            "lrnos": "",
            "invnos": "",
            "entryno": "",
            "copybills": "",
            "amount":0
        };
        $scope.Courier = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.ErrorMessage = "";

        $scope.Slide = { 'title': 'Communication', 'image': '' };
        $scope.AddSlide = function () {
            if ($scope.Courier.attachment == 'undefined' || $scope.Courier.attachment == null)
                $scope.Courier.attachment = [];
            $scope.Courier.attachment.push($scope.Slide);
        };
        $scope.ManagesearchSlideItem = function (action, item) {
            if (action == "DeleteSlide") {
                $scope.Courier.attachment.splice($scope.Courier.attachment.indexOf(item), 1);
            }
            else if (action == "DeletePOD") {
                $scope.Courier.podlist.splice($scope.Courier.podlist.indexOf(item), 1);
                $scope.$apply();
            }
        }

        $scope.SearchCondition = {
            companyid: "",
            type: -9,
            supplierid: "",
            status: "",
            awbnumber: "",
            podnumber: "",
            fromdate: null,
            todate: null
        }


        $scope.CourierList = [];

        $scope.Save = function () {

            if ($scope.Courier.status == null || $scope.Courier.status == 0) {
                $scope['setFocusstatus']();
                $rootScope.Toaster.Error("Please select Status");
                return;
            }
            if ($scope.Courier.receivingcompanyid == null || $scope.Courier.receivingcompanyid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please select Company");
                return;
            }
            if ($scope.Courier.companyid == null || $scope.Courier.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Error("Please select Courier Company");
                return;
            }

            if ($scope.CourierSetting.supplier && ( $scope.Courier.supplierid == null || $scope.Courier.supplierid == 0)) {
                $scope['setFocussupplierid']();
                $rootScope.Toaster.Error("Please select Supplier");
                return;
            }

            if ($scope.CourierSetting.customerb2b && ($scope.Courier.customerid == null || $scope.Courier.customerid == 0)) {
                $scope['setFocuscustomerb2b']();
                $rootScope.Toaster.Error("Please select Customer");
                return;
            }

            if ($scope.CourierSetting.customer && ($scope.Courier.customerid == null || $scope.Courier.customerid == 0)) {
                $scope['setFocuscustomerid']();
                $rootScope.Toaster.Error("Please select Customer");
                return;
            }

            if ($scope.CourierSetting.customerb2b && $scope.Courier.customerid && $scope.Courier.customerid > 0) {
                $scope.Courier.name = $scope.B2BCustomers.find(function (item) {
                    return item.id == $scope.Courier.customerid;
                }).name;
            }

            if ($scope.CourierSetting.supplier && $scope.Courier.supplierid && $scope.Courier.supplierid > 0) {
                $scope.Courier.name = $scope.Suppliers.find(function (item) {
                    return item.id == $scope.Courier.supplierid;
                }).name;
            }

            var request = {
                "Item": $scope.Courier,
                "securitytoken": "String content"
            }

            var isedit = typeof ($scope.Courier.id) != 'undefined' && $scope.Courier.id > 0;

            $http({
                method: 'POST',
                url: "CourierService/Save",
                data: request
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Courier Saved Successfully.!");
                    if (!isedit) {
                        $scope.CourierList.push(result);
                        if ($scope.CourierList.length == 1) {
                            $scope.CourierEditConfig();
                        }
                        $scope.egrid.UpdateData($scope.CourierList);
                    }
                    $scope.CreateNew();
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
                    content: 'Do you want to delete ' + item.awbnumber + "?",
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
                                    url: "CourierService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(request), 1);
                                        $scope.CourierList.splice($scope.CourierList.indexOf(request), 1);
                                        $scope.egrid.UpdateData($scope.CourierList);
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
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.Courier = item;
                var laststatus = $scope.Courier.status;
                $scope.Courier.status = null;
                if ($scope.Courier.mode == $scope.Modes.Inward) {
                    $scope.Status = $scope.InwardStatus;
                }
                else {
                    $scope.Status = $scope.OutwardStatus;
                }
                $timeout(function () {
                    $scope.Courier.status = laststatus;
                    $scope.ChangeConfiguration();
                }, 10)

            }
        }
        $scope.Search = function (id) {

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            if (typeof (id) != 'undefined')
                request.item = { 'id': id };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CourierService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof (id) != 'undefined' && result.length == 1) {
                    $scope.ShowSearch = false;
                    $scope.Courier = result[0];
                }
                else {
                    $scope.SearchResult = result;
                }
                $scope.CourierListConfig();
            }).error(function (result) {
            });
        }

        $scope.PrintCourier = function (id) {
            $rootScope.Download('CourierService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.Courier.id : id), 'Courier.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "courier.pdf";
                link.href = 'CourierService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.Courier.id : id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.DownloadImagePDF = function () {
            var rows = $scope.igrid.GetSelectedRows();
            var ids = [];
            $.each(rows, function (k, v) {
                ids.push(v.id);
            })
            if (ids.length == 0) return;

            request = {
                "item": {'ids':ids},
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CourierService/ImagePDFGeneration",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 16) {
                    $rootScope.Download("CourierService/ImagePDFGeneration/" + result, 'images.pdf', 'application/pdf');
                    return;
                }
            });
        }

        $scope.CourierListConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '300px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.SearchResult);
            igrid.AddSimpleColumn('entryno', 'Entry No', 60);
            igrid.AddReferenceColumn('type', 'Type', 100, $scope.TypesofCourier);
            var column = igrid.AddCustomColumn('Mode', 'Mode', 100, function (item, col) {
                return item.mode == $scope.Modes.Outward ? 'OutWard' : 'Inward';
            });

            var column = igrid.AddCustomColumn('Date', 'Date', 100, function (item, col) {
                return item.mode == 0 ? $filter('date')(item.receivedon, datePicker.shortDisplay) : $filter('date')(item.senton, datePicker.shortDisplay);
            });
            igrid.AddReferenceColumn('receivingcompanyid', 'Company', 100, $scope.Company);
            igrid.AddReferenceColumn('companyid', 'Courier', 100, $scope.CourierCompany);
            igrid.AddSimpleColumn('name', 'Name', 120);
            igrid.AddSimpleColumn('podnumber', 'POD Number', 100);
            igrid.AddSimpleColumn('ewbno', 'EWB No', 100);
            igrid.AddSimpleColumn('description', 'Description', 160);
            igrid.AddFormatedColumn('weight', 'Weight', 80, 'N2', 2);
            igrid.AddFormatedColumn('rate', 'Rate', 80, 'N2', 2);
            igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2);
            igrid.AddFormatedColumn('bundles', 'Bundles', 80, 'N2', 2);
            igrid.AddReferenceColumn('packedby', 'Packed By', 100, $scope.Employees);
            igrid.AddReferenceColumn('sentby', 'Booked By', 100, $scope.Employees);
       
            var commands = "";
            if ($rootScope.UserInfo.p_COURIER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_COURIER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            if ($rootScope.UserInfo.p_COURIER.print)
                commands += "<input type='button' class='btn btn-info btn-xs' value='Print'/>"

            if ($rootScope.UserInfo.p_COURIER.modify || $rootScope.UserInfo.p_COURIER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 140, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.PrintCourier(args.item.id);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }


        $scope.CourierEditConfig = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('gridedit', '450px', '100%', true, false, true, 280);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('entryno', 'Entry No', 60);
            igrid.AddReferenceColumn('type', 'Type', 100, $scope.TypesofCourier);
            var column = igrid.AddCustomColumn('Mode', 'Mode', 100, function (item, col) {
                return item.mode == $scope.Modes.Outward ? 'OutWard' : 'Inward';
            });

            var column = igrid.AddCustomColumn('Date', 'Date', 100, function (item, col) {
                return item.mode == 0 ? $filter('date')(item.receivedon, datePicker.shortDisplay) : $filter('date')(item.senton, datePicker.shortDisplay);
            });
            igrid.AddReferenceColumn('receivingcompanyid', 'Company', 100, $scope.Company);
            igrid.AddReferenceColumn('companyid', 'Courier', 100, $scope.CourierCompany);
            igrid.AddSimpleColumn('name', 'Name', 120);
            igrid.AddSimpleColumn('podnumber', 'POD Number', 100);
            igrid.AddSimpleColumn('ewbno', 'EWB No', 100);
            igrid.AddSimpleColumn('description', 'Description', 160);
            igrid.AddFormatedColumn('weight', 'Weight', 80, 'N2', 2);
            igrid.AddFormatedColumn('rate', 'Rate', 80, 'N2', 2);
            igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2);
            igrid.AddFormatedColumn('bundles', 'Bundles', 80, 'N2', 2);

            var commands = "";
            if ($rootScope.UserInfo.p_COURIER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_COURIER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            if ($rootScope.UserInfo.p_COURIER.print)
                commands += "<input type='button' class='btn btn-info btn-xs' value='Print'/>"

            if ($rootScope.UserInfo.p_COURIER.modify || $rootScope.UserInfo.p_COURIER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 140, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.PrintCourier(args.item.id);
                    });
                });

            }

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.egrid = igrid;
            $scope.egrid.UpdateData([]);
        }

        $timeout(function () {
            if (typeof ($location.search().id) != 'undefined') {
                $scope.Search($location.search().id);
            };

            //$scope.setFocusfirst();
        }, 10);


        $scope.ImageCaptured = function (img) {
            if (img && !angular.isArray(img))
                $scope.Slide.image = img;
            $scope.AddSlide();
            $scope.setFocuscameracapture();
        }

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

                    $scope.contactno = $scope.ngDialogData[0].Courier.customername;
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
                        $scope.Courier.customerid = data.value.id;
                        $scope.Courier.name = data.value.name;
                    }
                }
            });
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
        $scope.AddSlide = function () {
            $scope.Courier.attachment.push($scope.Slide);
            $scope.Slide = { 'title': '', 'image': '' };
        };

        //$scope.exporttoExcel = function (tableid, title) {
        //    $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'COURIER ENTRY REPORT');
        //    if ($scope.exportHref != null) {
        //        $timeout(function () {
        //            var link = document.createElement('a');
        //            link.download = filename + ".xls";
        //            link.href = $scope.exportHref;
        //            document.body.appendChild(link);
        //            link.click();
        //            link.remove();
        //        }, 100);
        //    }
        //}

        $scope.ModeChanged = function () {
            if ($scope.Courier.mode == 0) {
                $scope.Status = $scope.InwardStatus;
                $scope.Courier.status = 100;
            }
            else {
                $scope.Status = $scope.OutwardStatus;
                $scope.Courier.status = 102;
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            subtitle = 'COURIER REPORT ';
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

        $scope.CourierSetting = $scope.DefaultSettings;
        $scope.ChangeConfiguration = function () {
            if (typeof ($scope.Courier.type) == 'undefined' || $scope.Courier.type == -9) {
                $scope.CourierSetting = $scope.DefaultSettings;
            }
            else {
                var couriertype = $scope.TypesofCourier.find(function (item) {
                    return item.id == $scope.Courier.type
                });
                if (couriertype && couriertype != null) {
                    $scope.CourierSetting = couriertype.additionalinfo.courier
                }
                else {
                    $scope.CourierSetting = $scope.DefaultSettings;
                }
            }


            $scope.Courier.mode = $scope.CourierSetting.inward ? 0 : 1;
            if (typeof ($scope.Courier.id) == 'undefined' || $scope.Courier.id == null) {
                if ($scope.CourierSetting.couriercompanyid > 0)
                    $scope.Courier.companyid = $scope.CourierSetting.couriercompanyid;

                $scope.Courier.weight = $scope.CourierSetting.weight;
                $scope.Courier.rate = $scope.CourierSetting.rate;
                $scope.Courier.amount = $scope.CourierSetting.payment;
            }
            $scope.LayoutConfig[0].visible = $scope.CourierSetting.inward == $scope.CourierSetting.outward;
            $scope.LayoutConfig[1].visible = $scope.Courier.mode == 0;
            $scope.LayoutConfig[2].visible = $scope.Courier.mode != 0;
            $scope.LayoutConfig[3].visible = $scope.CourierSetting.customer;
            $scope.LayoutConfig[4].visible = $scope.CourierSetting.name;
            $scope.LayoutConfig[5].visible = $scope.CourierSetting.customerb2b;
            $scope.LayoutConfig[6].visible = $scope.CourierSetting.supplier;
            $scope.LayoutConfig[7].visible = $scope.CourierSetting.invoiceno;
            $scope.LayoutConfig[8].visible = $scope.CourierSetting.collectpayment;
            $scope.LayoutConfig[9].visible = $scope.CourierSetting.billcopy;
            $scope.LayoutConfig[10].visible = $scope.CourierSetting.detail;
            $scope.LayoutConfig[11].visible = $scope.CourierSetting.awbewb;
            $scope.LayoutConfig[12].visible = $scope.CourierSetting.pod;
        }

        $scope.LayoutConfig = [{ id: '0', name: 'mode', visible: true },
        { id: '1', name: 'receivedon', visible: true },
        { id: '2', name: 'senton', visible: true },
        { id: '3', name: 'customername', visible: true },
        { id: '4', name: 'personname', visible: true },
        { id: '5', name: 'customerb2b', visible: true },
        { id: '6', name: 'supplierid', visible: true },
        { id: '7', name: 'invoiceno', visible: true },
        { id: '8', name: 'collectpayment', visible: true },
        { id: '9', name: 'billcopy', visible: true },
        { id: '10', name: 'detail', visible: true },
        { id: '11', name: 'awbewb', visible: true },
        { id: '12', name: 'pod', visible: true }]

        $scope.SearchPOD = function () {

            if ($scope.Courier.podnumber && $scope.Courier.podnumber != '') {
                var request = {
                    "Item": $scope.Courier,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "CourierService/SearchBill",
                    data: srequest
                }).success(function (result) {
                    if (typeof ($scope.Courier.podlist) == 'undefined' || $scope.Courier.podlist == null)
                        $scope.Courier.podlist = [];

                    $.each(result, function (k, v) {
                        $scope.Courier.podlist.push(v);
                    });

                    if ($scope.Courier.podlist.length > 0 &&
                        $scope.Courier.podlist[0].customerid &&
                        $scope.Courier.podlist[0].customerid > 0) {
                        $scope.Courier.customerid = $scope.Courier.podlist[0].customerid;
                    }
                }).error(function (result) {
                });
            }
            else {
                $scope.setFocusattachmenttype();
            }
        }

        $scope.Focus = function (focusid) {
            var config = $scope.LayoutConfig.find(function (item) {
                return item.name == focusid;
            });
            if (config && config != null) {
                if (focusid == 'packedby')
                    config.visible = $scope.Courier.mode == 1;

                if (config.visible)
                    $scope['setFocus' + focusid]();
                else {
                    var startindex = $scope.LayoutConfig.indexOf(config);
                    for (; startindex < $scope.LayoutConfig.length; startindex++) {
                        if ($scope.LayoutConfig[startindex].visible) {
                            var fname = 'setFocus' + $scope.LayoutConfig[startindex].name;
                            if ($scope[fname])
                                $scope[fname]();
                            return;
                        }
                    }
                }
            }
            else {
                if (focusid == 'receivedon') {
                    if ($scope.Courier.mode == 0) {
                        $scope.setFocusreceivedon();
                    }
                    else {
                        $scope.setFocussenton();
                    }
                }
            }
        }

        $scope.tamountchanged = function (source) {
            var weight = parseFloat($scope.Courier.weight);
            var rate = parseFloat($scope.Courier.rate);
            var tax = parseFloat($scope.Courier.taxamount);
            if (isNaN(weight)) weight = 0;
            if (isNaN(rate)) rate = 0;
            if (isNaN(tax)) tax = 0;
            if (source != 'amount') {
                $scope.Courier.amount = (weight * rate) + tax;
            }
        }

        $scope.CreateNew = function () {
            $scope.Courier = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Courier.receivedon = new Date();
            $scope.Courier.senton = new Date();
            $scope.setFocustypeofcorier();
        };
        $timeout(function () {
            $scope.CreateNew();
            $scope.CourierEditConfig();
        }, 10);

    }]);


