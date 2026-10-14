
var app = angular.module('InfinityLab');

app.controller('CustomerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'Excel', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, Excel, restService, $timeout, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });


        $scope.SupplyTypes = [{ 'id': 'B2B', 'name': 'Business to Business' },
        { 'id': 'B2C', 'name': 'Business to Customer' },
        { 'id': 'SEZWP', 'name': 'SEZ with payment' },
        { 'id': 'SEZWOP', 'name': 'SEZ without payment' },
        { 'id': 'EXPWP', 'name': 'Export with Payment' },
        { 'id': 'EXPWOP', 'name': 'Export without Payment' },
        { 'id': 'DEXP', 'name': 'Deemed Export' }];

        $scope.Religions = restService.Get('ReferencelistService/RELIGION', true).then(function (results) {
            $scope.Religions = results;
        });

        $scope.AreaGroup1List = restService.Get('ReferencelistService/AREAGROUP1', true).then(function (results) {
            $scope.AreaGroup1List = results;
        });

        $scope.AreaGroup2List = restService.Get('ReferencelistService/AREAGROUP2', true).then(function (results) {
            $scope.AreaGroup2List = results;
        });

        $scope.AreaList = restService.Get('ReferencelistService/AREA', true).then(function (results) {
            $.each(results, function (k, v) {
                v.areaid = v.id;
            })
            $scope.AreaList = results;
        });

        $scope.Agents = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });

        $scope.Transport = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transport = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });
        $scope.District = restService.Get('ReferencelistService/DISTRICT', true).then(function (results) {
            $scope.District = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });

        restService.Get('ReferencelistService/CUSTOMERGROUP', true).then(function (results) {
            $scope.CustomerGroups = results;
        });
        

        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });

        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });

        $scope.Ports = restService.Get('ReferencelistService/PORT', true).then(function (results) {
            $scope.Ports = results;
        });

        $scope.StatusList = [{ id: 1, name: 'Unapproved' }
            , { id: 2, name: 'Approved' },
        { id: 3, name: 'Deactivated' }];

        $scope.PriceSources = [{ 'id': 0, 'name': 'Retail' },
        { 'id': 1, 'name': 'Whole Sale' },
        { 'id': 2, 'name': 'Franchise' },
        { 'id': 3, 'name': 'Category - Price List' },
        { 'id': 4, 'name': 'Custom - Price List' },
        { 'id': 5, 'name': 'Brand Mark Down - MRP' },
        { 'id': 6, 'name': 'Brand Mark UP - Cost' }];

        $scope.MRPSources = [{ 'id': 0, 'name': 'MRP' },
        { 'id': 1, 'name': 'Whole Sale' },
        { 'id': 2, 'name': 'Franchise' },
        { 'id': 3, 'name': 'Retail' }];

        $scope.CustomerType = [
            { 'id': 1, 'name': 'Employee' },
            { 'id': 2, 'name': 'Retail' },
            { 'id': 3, 'name': 'Provider' },
            { 'id': 4, 'name': 'Customer' },
            { 'id': 5, 'name': 'Tailoring' },
            { 'id': 6, 'name': 'Franchaise' },
            { 'id': 7, 'name': 'Agencies' },
            { 'id': 8, 'name': 'E-Commerce' },
            { 'id': 100, 'name': 'Persons' },
        ];

        $scope.Gender = [{ 'id': 1, 'name': 'Male' }, { 'id': 2, 'name': 'Female' }, { 'id': 3, 'name': 'Transgender' }];
        $scope.FamilyMemberType = [{ 'id': 1, 'name': 'Spouse' }, { 'id': 2, 'name': 'kids' }];

        $scope.Entity = {

            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "supportcredit": false,
            "creditlimit": 0,
            "creditrightsgivenby": 0,
            "mobileno": null,
            "customertype": 4,
            "name": null,
            "dateofbirth": null,
            "marriagedate": null,
            "loyelty": 0,
            "gstno": "",
            "gender": null,
            "area": null,
            "married": false,
            "malekids": 0,
            "femalekids": 0,
            "loyeltyno": "",
            "code": "",
            "categoryid": 0,
            "address": "",
            "cityid": 0,
            "districtid": 0,
            "stateid": 0,
            "countryid": 0,
            "portloadingid": 0,
            "portdischargeid": 0,
            "pincode": null,
            "contactperson": null,
            "bankaccountname": null,
            "bankaccountno": null,
            "bankifci": null,
            "billingname": "",
            "religionid": "",
            "family": []
        };

        $scope.totalbillvalue = 0;
        $scope.totalreward = 0;
        $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.Customer.family = [];

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Customer.family = [];
        }

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            customertype: "",
            name: "",
            mobileno: "",
            loyeltyno: "",
            supportcredit: false,
            religionid: ""
        }
        $scope.New = function () {
            $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Customer.family = [];
            $scope.SearchResult = [];
            $scope.ErrorMessage = "";
        }

        $scope.View = function (item) {
            $scope.SearchView(item.id);
        }

        $scope.ViewResult = [];
        $scope.SearchView = function (id) {

            request = {
                "item": { id: id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchCustomerBills",
                data: srequest
            }).success(function (result) {
                $scope.totalbillvalue = 0;
                $scope.totalreward = 0;
                if (result != 'undefined' && result.length > 0) {
                    $scope.ViewResult = result;
                    $.each($scope.ViewResult, function (k, v) {
                        $scope.totalbillvalue += v.billvalue;
                        $scope.totalreward += (v.loyelty - v.adjusted);
                    });
                }
                else
                    $scope.ViewResult = [];
            }).error(function (result) {
            });

        }

        $scope.Save = function () {

            if ($scope.Customer.customertype == null || $scope.Customer.customertype == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select CustomerType");
                return;
            }
            if ($scope.Customer.name == null || $scope.Customer.name == "") {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please fill out customer name");
                return;
            }

            if ($scope.Customer.customertype != 2) {
                if ($scope.Customer.mobileno == null || $scope.Customer.mobileno == "" ||
                    $scope.Customer.mobileno.length != 10) {
                    $scope['setFocusmobileno']();
                    $rootScope.Toaster.Error("Please fill out customer valid mobileno");
                    return;
                }
            }

            if ($scope.Customer.gstno != 'undefined' && $scope.Customer.gstno != null
                && $scope.Customer.gstno != '') {

                if ($scope.Customer.gstno.length != 15) {
                    $scope['setFocustan']();
                    $rootScope.Toaster.Error("Please Enter valid GST number.");
                    return;
                }

            //    var gstinformat = new RegExp('^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$');
            //    var gstinformat = new RegExp('^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$');
            //    if (!gstinformat.test($scope.Customer.gstno)) {
            //        $scope['setFocustan']();
            //        $rootScope.Toaster.Error("Please Enter valid GST number.");
            //        return;
            //    }
            }

            /*if ($scope.Customer.gender == null || $scope.Customer.gender == "") {
                $scope['setFocusgender']();
                $rootScope.Toaster.Error("Please fill out customer  gender");
                return;
            }*/

            var request = {
                "Item": $scope.Customer,
                "securitytoken": "String content"
            }

            //var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/Save",
                data: request
            }).success(function (result) {

                if (result != null && result.id > 0 && (result.ErrorMessage == null || result.ErrorMessage == '')) {
                    $rootScope.Toaster.Info("Customer Saved Successfully.!");
                    $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
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
                                    url: "CustomerService/Delete",
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
                $scope.Customer = item;
                $scope.Navigation.SetMode('EDIT');
            }
            else if (action == "Activate") {
                if (item.isactive == false) {
                    request = {
                        "Item": item,
                        "SecurityToken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "CustomerService/Activate",
                        data: srequest
                    }).success(function (result) {

                        if (result) {
                            $scope.ErrorMessage = "Successfully activated.";
                        }

                    }).error(function (result) {

                    });
                }
            }
        }

        $scope.Search = function () {

            if (($scope.SearchCondition.mobileno == null || $scope.SearchCondition.mobileno == '') &&
                ($scope.SearchCondition.loyeltyno == null || $scope.SearchCondition.loyeltyno == '') &&
                ($scope.SearchCondition.name == null || $scope.SearchCondition.name == '') &&
                ($scope.SearchCondition.religionid == null || $scope.SearchCondition.religionid == '')) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please enter mobile no or name.");
                return;
            }

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 20000)
                    $rootScope.Toaster.Info("Please refine your search criteria to get more appropriate records.");
                $scope.totalbillvalue = 0;
                $scope.totalreward = 0;
                $scope.SearchResult = result;
                $scope.CustomerConfig();
                if (result != null && result.length == 1) {
                    $scope.ViewResult = result[0].customerbill;
                    if ($scope.ViewResult == 'undefined' || $scope.ViewResult == null)
                        return;
                    $.each($scope.ViewResult, function (k, v) {
                        $scope.totalbillvalue += v.billvalue;
                        $scope.totalreward += (v.loyelty - v.adjusted);
                    });
                }
                else
                    $scope.ViewResult = [];
            }).error(function (result) {
            });

        }
        $scope.PrintMyCustomers = function () {

            $scope.CustomerList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/PrepareList",
                data: srequest
            }).success(function (result) {

                $rootScope.Download('CustomerService/PrintLabel/' + $scope.UserInfo.OrganizationID + '/' + result, 'customerlabel.pdf', 'application/pdf');
                return;

                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "customerlabel.pdf";
                    link.href = 'CustomerService/PrintLabel/' + $scope.UserInfo.OrganizationID + '/' + result;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);

            }).error(function (result) {

            });
        }

        $scope.PrintSticker = function () {
            var ids = [];
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length == 0) return;

            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Info("Please select customer to print.");
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
                    url: "CustomerService/PrintCustomerLabel",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.AssignAreaID = function () {
            var area = $scope.AreaList.find(function (item) {
                return item.name == $scope.Customer.area;
            });
            if (area && area != null)
                $scope.Customer.areaid = area.areaid;
            else
                $scope.Customer.areaid = 0;
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;
        }

        $scope.CustomerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('customertype', 'Type', 80, $scope.CustomerType);
            igrid.AddReferenceColumn('pricesource', 'Price Source', 80, $scope.PriceSources);
            igrid.AddSimpleColumn('code', 'Code', 80);
            igrid.AddSimpleColumn('name', 'Name', 150);
            igrid.AddSimpleColumn('billingname', 'Billing Name', 100);
            igrid.AddSimpleColumn('mobileno', 'Mobile Number', 100);
            igrid.AddSimpleColumn('gstno', 'GST No', 120);
            igrid.AddSimpleColumn('cityname', 'City', 100);
            igrid.AddSimpleColumn('area', 'Area', 100);
            igrid.AddSimpleColumn('address', 'Address', 180);
            igrid.AddSimpleColumn('pincode', 'Pincode', 180);
            	
            igrid.AddSimpleColumn('creditlimit', 'Credit Limit ', 80);

            if ($rootScope.UserInfo.appsettings.system_ismfgapplication) {
                igrid.AddSimpleColumn('otherno', 'Contact No', 80);
                igrid.AddReferenceColumn('agentid', 'Agent', 80, $scope.Agents);
                igrid.AddReferenceColumn('preferredtransportid', 'Transport', 80, $scope.Transport);
            }
            else {
                igrid.AddReferenceColumn('religionid', 'Religion', 80, $scope.Religions);
                igrid.AddSimpleColumn('loyeltyno', 'Card No ', 80);
                igrid.AddSimpleColumn('loyelty', 'Loyalty', 80);
            }
            var commands = "";
            if ($rootScope.UserInfo.p_CUSTOMERMASTER.modify) 
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_CUSTOMERMASTER.modify && $scope.SearchCondition.inactive)
                commands += "<input type='button' class='btn btn-info btn-xs' value='Activate'/> &nbsp;"

            if ($rootScope.UserInfo.p_CUSTOMERMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_CUSTOMERMASTER.modify || $rootScope.UserInfo.p_CUSTOMERMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 150, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Activate')
                            $scope.ManageSearchItem('Activate', args.item);
                    });
                });
            }


            igrid.options.autoEdit = true;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.selectedcount = 0;
        $scope.SaveFamilyDetail = function () {
            if (typeof ($scope.Customer.family) == 'undefined' || $scope.Customer.family == null)
                $scope.Customer.family = [];
            $scope.Customer.family.push(
                {
                    'name': $scope.Customer.kid.name,
                    'relation': $scope.Customer.kid.relation,
                    'gender': $scope.Customer.kid.gender,
                    'dateofbirth': $scope.Customer.kid.dateofbirth
                });
            $scope.Customer.kid.name = '';
            $scope.Customer.kid.gender = 0;
            $scope.Customer.kid.relation = 0;
            $scope.Customer.kid.dateofbirth = new Date();
        }


        $scope.DeletefromList = function (source, removeitem) {
            //source.splice(source.indexOf(removeitem), 1);
            $scope.Customer.family.splice($scope.Customer.family.indexOf(removeitem), 1);
        }

        $scope.ManageAddress = function () {

            restService.Get('SupplierService/GSTNo/' + $scope.Customer.gstno, true).then(function (result) {

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
                        $scope.Customer = (JSON.parse(JSON.stringify($scope.ngDialogData.scope.Customer)));
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
                            $scope.Customer.name = $scope.gst.tradeNam;
                            $scope.Customer.address = $scope.gst.pradr.addr.address;
                            var existingcity = $scope.Cities.find(function (item) {
                                return item.name.toUpperCase() == $scope.gst.pradr.addr.city.toUpperCase();
                            });
                            if (typeof (existingcity) != 'undefined' && existingcity != null) {
                                $scope.Customer.cityid = existingcity.id;
                            }

                            var existingstate = $scope.States.find(function (item) {
                                return item.name.toUpperCase() == $scope.gst.pradr.addr.stcd.toUpperCase();
                            });
                            if (typeof (existingstate) != 'undefined' && existingstate != null) {
                                $scope.Customer.stateid = existingstate.id;
                            }
                            $scope.Customer.pincode = $scope.gst.pradr.addr.pncd;
                        }

                        $scope.Selected = function () {
                            $scope.closeThisDialog($scope.Customer);
                        };
                        $scope.addresschanged();
                    }],
                    className: 'ngdialog-theme-plain'
                });

                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null
                            && data.value != '$escape' && data.value != '$document') {
                            $scope.Customer.name = data.value.name;
                            $scope.Customer.address = data.value.address;
                            $scope.Customer.cityid = data.value.cityid;
                            $scope.Customer.stateid = data.value.stateid;
                            $scope.Customer.pincode = data.value.pincode;
                        }
                    }
                });

            });
        }


        $scope.AddDeliveryAddress = function () {
            var dialog = ngDialog.open({
                template: 'ShowDeliveryAddress',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.Customer = $scope.Parent.Customer;
                    $scope.SaveFamilyDetail = $scope.Parent.SaveFamilyDetail;
                    $scope.DeletefromList = function (source, removeitem) {
                        $scope.Customer.deliveryaddresslist.splice($scope.Customer.deliveryaddresslist.indexOf(removeitem), 1);
                    }
                    $scope.SaveDeliveryAddress = function () {
                        if (typeof ($scope.Customer.deliveryaddresslist) == 'undefined' || $scope.Customer.deliveryaddresslist == null)
                            $scope.Customer.deliveryaddresslist = [];
                        $scope.Customer.deliveryaddresslist.push({ 'name': $scope.name, 'address': $scope.address, 'cityid': $scope.cityid });
                        $scope.name = '';
                        $scope.address = '';
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                }
            });
        };

        $scope.AddDocuments = function () {
            var dialog = ngDialog.open({
                template: 'Attachment',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', 'restService', function ($scope, $timeout, $http, $rootScope, restService) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.Customer = $scope.Parent.Customer;

                    if ($scope.Customer.attachment == undefined ||
                        $scope.Customer.attachment == null)
                        $scope.Customer.attachment = [];

                    $scope.DocTypes = restService.Get('ReferencelistService/DOCTYPE', true).then(function (results) {
                        if (results.length == 0)
                            results.push({ id: -1,name: 'General'});
                        $scope.DocTypes = results;
                    });
                    $scope.Slide = { 'title': '', 'image': '', 'typename': '' };
                    $scope.AddSlide = function () {

                        var _item = $scope.DocTypes.find(function (item) {
                            return item.id == $scope.Slide.title;
                        });
                        if (_item != 'undefined' && _item != null) {
                            $scope.Slide.typename = _item.name;
                        }

                        $scope.Customer.attachment.push($scope.Slide);
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
                                            $scope.Customer.attachment.splice($scope.Customer.attachment.indexOf(item), 1);
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
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                }
            });
        };

        $scope.AddKids = function () {
            var dialog = ngDialog.open({
                template: 'ShowKids',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.Customer = $scope.Parent.Customer;
                    $scope.SaveFamilyDetail = $scope.Parent.SaveFamilyDetail;
                    $scope.DeletefromList = $scope.Parent.DeletefromList;
                    $scope.Gender = $scope.Parent.Gender;
                    $scope.FamilyMemberType = $scope.Parent.FamilyMemberType;
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                }
            });
        };

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
                            url: "CustomerService/ChangeStatus",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Error('Attributes successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.Upload2CRM = function () {
            var request = {
                "Items": $scope.igrid.GetSelectedRows(),
                "securitytoken": "String content"
            }
            $http({
                method: 'POST',
                url: "CustomerService/CRMIntegration",
                data: request
            }).success(function (result) {

            }).error(function (result) {
            });
        }

        //{ 'id': 1, 'name': 'Employee' },
        //{ 'id': 2, 'name': 'Retail' },
        //{ 'id': 3, 'name': 'Provider' },
        //{ 'id': 4, 'name': 'Customer' },
        //{ 'id': 5, 'name': 'Tailoring' },
        //{ 'id': 6, 'name': 'Franchaise' },
        //{ 'id': 7, 'name': 'Agencies' }

        $scope.pricesourcefocus = function () {
            if (($scope.Customer.customertype) &&
                ($scope.Customer.customertype == 2 ||
                    $scope.Customer.customertype == 3 ||
                    $scope.Customer.customertype == 6 ||
                    $scope.Customer.customertype == 7)) {
                $scope.setFocuspricesource();
            }
            else {
                if ($scope.UserInfo &&
                    $scope.UserInfo.p_HIDECUSTOMERCAT &&
                    $scope.UserInfo.p_HIDECUSTOMERCAT.view) {
                    $scope.setFocuscode();
                }
                else {
                    $scope.setFocusCustomerCategory();
                }
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);


            if (tableid == 'Customerreport')
                subtitle = 'CUSTOMER DETAILS';


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
                            url: "CustomerService/Merge",
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
                                $scope.selectedcount = 0;
                            }
                        });
                    }
                }
            });
        }


        $timeout(function () {

            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "CustomerService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {

                        $scope.ShowSearch = false;
                        $scope.Customer = result[0];
                        $scope.Navigation.SetMode('EDIT');

                    }

                }).error(function (result) {
                });
            }
            $scope.setFocusfirst();
        }, 100);

    }]);