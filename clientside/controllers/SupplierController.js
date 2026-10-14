
var app = angular.module('InfinityLab');

app.controller('SupplierController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, localStorageService) {

        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.selectedcount = 0;
        $scope.ReferenceTypeList = [{ 'id': 1, 'text': 'COUNTRY' }, { 'id': 2, 'text': 'STATE' }, { 'id': 3, 'text': 'CITY' },
        { 'id': 4, 'text': 'BRAND' }, { 'id': 5, 'text': 'PATTERN' }, { 'id': 6, 'text': 'SIZE' }, { 'id': 7, 'text': 'STYLE' },
        { 'id': 8, 'text': 'LOCATION' }, { 'id': 9, 'text': 'DESIGNATION' }, { 'id': 10, 'text': 'FRIGHTCHARGE' }];

        $scope.RowTypes = [{ 'id': 0, 'name': 'SUPPLIER' }, { 'id': 1, 'name': 'JOB WORKER' },
        { 'id': 2, 'name': 'AGENT' }];

        $scope.StatusList = [{ id: 1, name: 'Unapproved' }
            , { id: 2, name: 'Approved' },
        { id: 3, name: 'Rejected' }];

        $scope.RowTypeList = [{ 'id': -1, 'name': 'ALL SUPPLIER' }, { 'id': 0, 'name': 'SUPPLIER' }, { 'id': 1, 'name': 'JOB WORKER' },
        { 'id': 2, 'name': 'AGENT' }];

        $scope.DocTypes = restService.Get('ReferencelistService/DOCTYPE', true).then(function (results) {
            $scope.DocTypes = results;
        });

        $scope.WarehouseLocations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
        });
        $scope.AddressTypes = restService.Get('ReferencelistService/ADDRESSTYPE', true).then(function (results) {
            $scope.AddressTypes = results;
        });
        $scope.SupplierGroups = restService.Get('ReferencelistService/SUPPLIERGROUP', true).then(function (results) {
            $scope.SupplierGroups = results;
        });
        $scope.TDSGroups = restService.Get('ReferencelistService/TDSLEDGERS', true).then(function (results) {
            $scope.TDSGroups = results;
        });
        $scope.BuyerGroup = restService.Get('ReferencelistService/BUYERSGROUP', true).then(function (results) {
            $scope.BuyerGroup = results;
        });
        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });
        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });

        $scope.TransportList = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.TransportList = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });

        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });

        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });


        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;

            $scope.Suppliers = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SupplierCompany = (JSON.parse(JSON.stringify($scope.EntityItem)));
        }

        // model for User creation
        $scope.Entity = {
            "id": 0, "name": null, "contactperson": null, "contactnumber": null, "emailid": null,
            creditdays: "", "locationid": 0, "isactive": true, "createdby": 0, "modifiedby": 0, isactive: true,
            "version": 0, "createdon": "0001-01-01T00:00:00", "modifiedon": "0001-01-01T00:00:00", "companylist": [],
            "product": [], "brand": [], code: "", discountpercentage: "", isunregistered: "", activatedon: "", deactivatedon: "",
            remarks: "", rowtype: 0

        };
        $scope.EntityItem = {
            "id": 0, "supplierid": 0, "address": null, "cityid": 0, "stateid": 0, "countryid": 0,
            "pincode": null, "contactperson": null, "tallyledger": null, "agentid": 0, "companycontactno": null,
            "companyemailid": null, "tinanddate": null, "cstanddate": null, "areacode": null, "tan": null, "pan": null,
            "bankaccountname": null, "bankaccountno": null, "bankifci": null, "isactive": true, "createdby": 0, "modifiedby": 0,
            "version": 1, "createdon": "0001-01-01T00:00:00", "modifiedon": "0001-01-01T00:00:00", "name": null, isactive: true,
            "attachment": [], "addresslist": []
        };

        $scope.Suppliers = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.SupplierCompany = (JSON.parse(JSON.stringify($scope.EntityItem)));
        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            contactnumber: "",
            cityid: 0,
            agentid: 0,
            inactive: false,
            rowtype: -1
        }
        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });
        $scope.AlternativeSuppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.AlternativeSuppliers = results;
        });
        $scope.New = function () {
            $scope.Suppliers = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SupplierCompany = (JSON.parse(JSON.stringify($scope.EntityItem)));
        };

        $scope.Addlists = {
            productname: "",
            brandname: "'"
        }

        $scope.Slide = { 'title': '', 'image': '', 'typename': '' };
        $scope.AddressItem = { 'typeid': 0, 'contactnumber': '', 'address': '', 'cityid': 0, 'countryid': 0, 'pincode': '', 'typename': '' };

        $scope.AddSlide = function () {

            var _item = $scope.DocTypes.find(function (item) {
                return item.id == $scope.Slide.title;
            });
            if (_item != 'undefined' && _item != null) {
                $scope.Slide.typename = _item.name;
            }

            $scope.SupplierCompany.attachment.push($scope.Slide);
            $scope.Slide = { 'title': '', 'image': '', 'typename': '' };
        };

        $scope.ManagesearchSlideItem = function (action, item) {
            if (action == "DeleteSlide") {
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
                                $scope.SupplierCompany.attachment.splice($scope.SupplierCompany.attachment.indexOf(item), 1);
                                $scope.$apply();
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
            else if (action == "EditSlide") {
                $scope.Slide = item;
            }
        }

        $scope.AddAddress = function () {
            var _item = $scope.AddressTypes.find(function (item) {
                return item.id == $scope.AddressItem.typeid;
            });
            if (_item != 'undefined' && _item != null) {
                $scope.AddressItem.typename = _item.name;
            }

            $scope.SupplierCompany.addresslist.push($scope.AddressItem);
            $scope.AddressItem = { 'typeid': 0, 'contactnumber': '', 'address': '', 'cityid': 0, 'countryid': 0, 'pincode': '', 'typename': '' };
        };

        $scope.ManagesearchAddressItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.typename + "?",
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
                                $scope.SupplierCompany.addresslist.splice($scope.SupplierCompany.addresslist.indexOf(item), 1);
                                $scope.$apply();
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
                $scope.AddressItem = item;
            }
        }

        $scope.ManageBrands = function () {
            if (typeof ($scope.Suppliers.brand) == 'undefined' || $scope.Suppliers.brand == null)
                $scope.Suppliers.brand = [];
            $scope.Suppliers.brand.push(
                {
                    'id': $scope.Addlists.brandname
                });
        }
        $scope.ManageProducts = function () {
            if (typeof ($scope.Suppliers.product) == 'undefined' || $scope.Suppliers.product == null)
                $scope.Suppliers.product = [];
            $scope.Suppliers.product.push(
                {
                    'id': $scope.Addlists.productname
                });
        }

        $scope.ManageList = function (action, item) {
            if (action == "Deletebrand") {
                $scope.Suppliers.brand.splice($scope.Suppliers.brand.indexOf(item), 1);
            }
            if (action == "Deleteproduct") {
                $scope.Suppliers.product.splice($scope.Suppliers.product.indexOf(item), 1);
            }
        }

        $scope.Save = function () {

            if ($scope.Suppliers.name == null || $scope.Suppliers.name == 0) {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please enter Supplier Name");
                return;
            }
            if ($scope.SupplierCompany.name == null || $scope.SupplierCompany.name == 0) {
                $scope['setFocuscompanyname']();
                $rootScope.Toaster.Error("Please enter Supplier Company Name");
                return;
            }
            if ($scope.Suppliers.contactperson == null || $scope.Suppliers.contactperson == 0) {
                $scope['setFocuscontactperson']();
                $rootScope.Toaster.Error("Please select ContactPerson");
                return;
            }
            if ($scope.Suppliers.contactnumber == null || $scope.Suppliers.contactnumber == 0) {
                $scope['setFocuscontactnumber']();
                $rootScope.Toaster.Error("Please select Number");
                return;
            }

            if ($scope.Suppliers.isunregistered == undefined ||
                $scope.Suppliers.isunregistered == null ||
                $scope.Suppliers.isunregistered == false) {
                if ($scope.SupplierCompany.tinanddate == undefined ||
                    $scope.SupplierCompany.tinanddate == null ||
                    $scope.SupplierCompany.tinanddate == '') {
                    $scope['setFocustinanddate']();
                    $rootScope.Toaster.Error("Please enter the gst no / change to URD(Un Registred)");
                    return;
                }
            }

            $scope.SupplierCompany.isactive = true;
            $scope.Suppliers.cityid = $scope.SupplierCompany.cityid;
            $scope.Suppliers.companylist = [];
            $scope.Suppliers.companylist.push($scope.SupplierCompany);

            if ($scope.SupplierCompany.tinanddate != 'undefined' && $scope.SupplierCompany.tinanddate != null
                && $scope.SupplierCompany.tinanddate != '') {

                if ($scope.SupplierCompany.tinanddate.length != 15) {
                    $scope['setFocustinanddate']();
                    $rootScope.Toaster.Error("Please Enter valid GST number.");
                    return;
                }

                //var gstinformat = new RegExp('^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$');
                //var gstinformat = new RegExp('^[0-9]{2}[A-Z]{4}[0-9][0-9]{4}[A-Z][0-9A-Z][A-Z][0-9A-Z]$');
                //if (!gstinformat.test($scope.SupplierCompany.tinanddate)) {
                //    $scope['setFocustinanddate']();
                //    $rootScope.Toaster.Error("Please Enter valid GST number.");
                //    return;
                //}
            }
            //if ($scope.Suppliers.companylist != null && typeof($scope.Suppliers.companylist) != 'undefined') {
            //    $.each($scope.Suppliers.companylist, function (k, v) {
            //        v.isactive = $scope.Suppliers.isactive;
            //    });
            //}

            request = {
                "Item": $scope.Suppliers,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    if (result.ErrorMessage != 'undefined' && result.ErrorMessage != null && result.ErrorMessage != '')
                        $rootScope.Toaster.Error(result.ErrorMessage);
                    else {
                        $rootScope.Toaster.Info("Supplier saved Successfully ");
                        $scope.Suppliers = (JSON.parse(JSON.stringify($scope.Entity)));
                        $scope.SupplierCompany = (JSON.parse(JSON.stringify($scope.EntityItem)));
                    }
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

        //$scope.ManageCompanyItem = function (action, item) {
        //    if (action == "Delete") {
        //        $scope.Suppliers.companylist.splice($scope.Suppliers.companylist.indexOf(item), 1);
        //    } else if (action == "Edit") {
        //        $scope.SupplierCompany = item;
        //        $scope.Suppliers.companylist.splice($scope.Suppliers.companylist.indexOf(item), 1);
        //    }
        //};

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
                                    url: "SupplierService/Delete",
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
                if (typeof ($scope.Suppliers.companylist) != 'undefined' && angular.isArray($scope.Suppliers.companylist))
                    $scope.SupplierCompany = $scope.Suppliers.companylist[0];

                request = {
                    "Item": { id: item.id },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SupplierService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {
                        $scope.Suppliers = result[0];
                        if (typeof ($scope.Suppliers.companylist) != 'undefined' && angular.isArray($scope.Suppliers.companylist))
                            $scope.SupplierCompany = $scope.Suppliers.companylist[0];
                    }

                    $scope.Navigation.SetMode('EDIT');
                }).error(function (result) {
                });
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
                url: "SupplierService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 0) {
                    $rootScope.Toaster.Info("No Results found.");
                }
                $scope.SearchResult = result;
                $scope.SupplierConfig();
            }).error(function (result) {
            });

        }
        $scope.PrintLabel = function () {

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierService/PrepareList",
                data: srequest
            }).success(function (result) {
                $rootScope.Download('SupplierService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + result, 'supplierlabel.pdf', 'application/pdf');
                return;

                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "supplierlabel.pdf";
                    link.href = 'SupplierService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + result;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);

            }).error(function (result) {

            });
        }

        $scope.SupplierConfig = function () {
            $scope.selectedcount = 0;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('activestatus', 'Status', 100, $scope.StatusList);
            igrid.AddSimpleColumn('code', 'Supplier Code', 120);
            igrid.AddSimpleColumn('name', 'Supplier Name', 200);
            igrid.AddSimpleColumn('companyname', 'Reg.Name', 200);
            igrid.AddReferenceColumn('cityid', 'City', 120, $scope.Cities);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 60, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('address', 'Address', 180);
            igrid.AddSimpleColumn('contactnumber', 'Mobile', 100);
            igrid.AddSimpleColumn('emailid', 'Mail', 100);
            igrid.AddFormatedColumn('creditdays', 'Credit Days', 80, 'N2', 0);
            igrid.AddSimpleColumn('tinanddate', 'GST NO', 120);
            igrid.AddSimpleColumn('msmeno', 'MSME NO', 100);
            igrid.AddCustomColumn('isunregistered', 'URD', 60, function (item, col) {
                return item.isunregistered == true ? "YES" : "NO";
            });
            igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.Agent);
            igrid.AddReferenceColumn('preferredtransportid', 'Transport', 100, $scope.TransportList);

            var commands = "";
            if ($rootScope.UserInfo.p_SUPPLIERMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_SUPPLIERMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_SUPPLIERMASTER.modify || $rootScope.UserInfo.p_SUPPLIERMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    $scope.$apply();
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');

        }

        $scope.Export = function () {
            if (typeof ($scope.igrid) == 'undefined') return;
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Supplier List');
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

        $scope.PrintSticker = function () {
            var ids = [];
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length == 0) return;

            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Info("Please select supplier to print.");
                return;
            }
            $scope.ChoseReport({ 'ids': ids });
        };

        $scope.ChoseReport = function (requestitem) {
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
                    url: "SupplierService/PrintSupplierLable",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;
        }

        $scope.ManageAddress = function () {

            restService.Get('SupplierService/GSTNo/' + $scope.SupplierCompany.tinanddate, true).then(function (result) {

                result.pradr.id = -1;
                result.pradr.name = 'Present';
                result.adadrlist = [];
                result.adadrlist.push(result.pradr);
                $.each(result.adadr, function (k, v) {
                    v.id = k + 1;
                    v.name = 'Branch - ' + (k + 1);
                    result.adadrlist.push(v);
                });

                var dialog = ngDialog.open({
                    template: 'ManageAddress',
                    data: {
                        'scope': $scope,
                        'taxpayer': result
                    },
                    width: '700px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.SupplierCompany = (JSON.parse(JSON.stringify($scope.ngDialogData.scope.SupplierCompany)));
                        $scope.Cities = $scope.ngDialogData.scope.Cities;
                        $scope.States = $scope.ngDialogData.scope.States;
                        $scope.gst = $scope.ngDialogData.taxpayer;
                        $scope.gst.pradrid = -1;
                        $scope.addresschanged = function () {
                            var addr = $scope.gst.adadrlist.find(function (item) {
                                return item.id == $scope.gst.pradrid;
                            });
                            if (typeof (addr) != 'undefined' && addr != null) {
                                $scope.gst.pradr = addr;
                            }
                        }
                        $scope.Apply = function () {
                            $scope.SupplierCompany.name = $scope.gst.tradeNam;
                            $scope.SupplierCompany.address = $scope.gst.pradr.addr.address;
                            var existingcity = $scope.Cities.find(function (item) {
                                return item.name.toUpperCase() == $scope.gst.pradr.addr.city.toUpperCase();
                            });
                            if (typeof (existingcity) != 'undefined' && existingcity != null) {
                                $scope.SupplierCompany.cityid = existingcity.id;
                            }

                            var existingstate = $scope.States.find(function (item) {
                                return item.name.toUpperCase() == $scope.gst.pradr.addr.stcd.toUpperCase();
                            });
                            if (typeof (existingstate) != 'undefined' && existingstate != null) {
                                $scope.SupplierCompany.stateid = existingstate.id;
                            }
                            $scope.SupplierCompany.pincode = $scope.gst.pradr.addr.pncd;

                        }

                        $scope.Selected = function () {
                            $scope.closeThisDialog($scope.SupplierCompany);
                        };
                        $scope.addresschanged();
                    }],
                    className: 'ngdialog-theme-plain'
                });

                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null
                            && data.value != '$escape' && data.value != '$document') {
                            $scope.SupplierCompany.name = data.value.name;
                            $scope.SupplierCompany.address = data.value.address;
                            $scope.SupplierCompany.cityid = data.value.cityid;
                            $scope.SupplierCompany.stateid = data.value.stateid;
                            $scope.SupplierCompany.pincode = data.value.pincode;
                        }
                    }
                });

            });
        }

        $scope.RowTypeChanged = function () {
            $scope.Suppliers.supportsourcesupplier = $scope.Suppliers.rowtype == 2;
        }

        $scope.ShowStatusDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangeStatusDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.statusid);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = { 'statusid': data.value, ids: [] };

                        $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                            request.ids.push(v.id);
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "SupplierService/ChangeStatus",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Error('Attributes successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ShowAccessDialog = function () {
            var dialog = ngDialog.open({
                template: 'APIAccessDialog',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', 'restService', '$http',
                    function ($scope, $timeout, restService, $http) {
                        $scope.parent = $scope.ngDialogData[0];
                        $scope.entity = {};
                        restService.Get('ReferencelistService/ACCESSKEY/SUPPLIER/' + $scope.parent.Suppliers.id, true).then(function (results) {
                            $scope.entity = results;
                        });
                        $scope.Create = function () {
                            restService.Get('ReferencelistService/GENACCESSKEY/SUPPLIER/' + $scope.parent.Suppliers.id, true).then(function (results) {
                                if ($scope.entity.lastAccess &&
                                    $scope.entity.lastAccess != null) {
                                    results.lastAccess = $scope.entity.lastAccess;
                                }
                                $scope.entity = results;
                            });
                        };
                        $scope.Revoke = function () {
                            request = {
                                "item": $scope.entity,
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "ReferencelistService/REVOKEACCESSKEY",
                                data: srequest
                            }).success(function (result) {
                                $scope.entity = {
                                    'id': 0,
                                    'status': 0,
                                    'lastAccess': result
                                };
                            });
                        };
                        $scope.ChangeStock = function () {
                            $scope.closeThisDialog();
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                }
            });
        }

        $scope.MergeSelected = function () {
            var dialog = ngDialog.open({
                template: 'ChooseMerge',
                data: {
                    'rows': $scope.igrid.GetSelectedRows(),
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                    $scope.SelectedItem = null;
                    $timeout(function () {
                        $scope.id = 0;
                    }, 10);
                    $scope.rowselected = function () {
                        $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                        if (typeof ($scope.id) != 'undefined' && $scope.id > 0) {
                            var selected = $scope.SelectedRows.find(function (item) {
                                return item.id == $scope.id;
                            });
                            $scope.SelectedItem = selected;
                        }
                    }
                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'selectedid': $scope.id });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        request = {
                            "item": {
                                'id': data.value.selectedid,
                                'deletelist': $scope.igrid.GetSelectedRows()
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "SupplierService/Merge",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                var rows = $scope.igrid.GetSelectedRows();
                                $.each(rows, function (k, v) {
                                    if (v.id != data.value.selectedid) {
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(v), 1);
                                    }
                                });
                                $scope.igrid.UpdateData($scope.SearchResult, '_id');
                            }
                        });
                    }
                }
            });
        }

        $timeout(function () {
            $scope.setFocusfirst();
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SupplierService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {
                        $scope.Suppliers = result[0];
                        if (typeof ($scope.Suppliers.companylist) != 'undefined' && angular.isArray($scope.Suppliers.companylist))
                            $scope.SupplierCompany = $scope.Suppliers.companylist[0];
                    }

                }).error(function (result) {
                });
            }
        }, 100);

    }]);