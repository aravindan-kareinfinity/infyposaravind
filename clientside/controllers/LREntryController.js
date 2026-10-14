var app = angular.module('InfinityLab');
app.controller('LREntryController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'Excel', 'ngDialog','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, ngDialog, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSourceSupplier = false;
        $scope.LocationConfig = { 'label': 'Auto transfer Location', 'showwarehouse': false, 'showentrylocation': false, 'filterlocation': false };
        $scope.SupplierClass = 'col-md-8';
        $scope.FrieghtChargesTypes = [{ 'id': 1, 'name': 'No Charge', 'type': false }, { 'id': 2, 'name': 'To Pay', 'type': true }];
        $scope.ImageTypes = [{ 'id': 'LR', 'name': 'LR' },
        {
            'id': 'Invoices', 'name': 'Invoices'
        },
        {
            'id': 'Debit Note', 'name': 'Debit Note'
        }, {
            'id': 'Communication', 'name': 'Communication'
        }, {
            'id': 'Import Excel', 'name': 'Import Excel'
        }, {
            'id': 'Import Quanto Bill', 'name': 'Import Quanto Bill'
        }, {
            'id': 'Import Quanto Excel', 'name': 'Import Quanto Excel'
        }];
        $scope.PayModes = [{ 'id': 1, 'name': 'Immediate' }, { 'id': 2, 'name': 'Agreed Days' }, { 'id': 3, 'name': 'High' }
            , { 'id': 4, 'name': 'Medium' }, { 'id': 5, 'name': 'Low' }, { 'id': 6, 'name': 'After Sales' }];

        $scope.ModeofDelivery = [{ 'id': 1, 'name': 'Lorry' }, { 'id': 2, 'name': 'Hand Delivery' },
            { 'id': 3, 'name': 'Tailoring' }, { 'id': 4, 'name': 'Delivery Challan' },
            { 'id': 5, 'name': 'Temporary' }, { 'id': 6, 'name': 'MFG - Jobwork' },
            { 'id': 7, 'name': 'MFG - BOM' }, { 'id': 8, 'name': 'Inter Transfer' },
            { 'id': 9, 'name': 'Expenses' }, { 'id': 10, 'name': 'RCM - Transports' }];

        $scope.FrieghtChargeList = [];
        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transports = results;
        });
        $scope.WarehouseLocations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
        });

        $scope.BillImports = restService.Get('ReferencelistService/BILLIMPORT', true).then(function (results) {
            $scope.BillImports = results;
            results.push({ 'id': -2, 'name': 'Custom Format' });
        });
        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });
        $scope.RetialLocations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.RetialLocations = results;
            results.push({ 'id': 0, 'name': 'NONE' });
            results.push({ 'id': -1, 'name': 'Multiple Locations' });
            $scope.AllowedRetialLocations = $scope.RetialLocations;
        });

        $scope.FrieghtCharges = restService.Get('ReferencelistService/FRIGHTCHARGE', true).then(function (results) {
            $scope.LoadFrightCharges(results);
        });
        $scope.ReceivingLocations = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.ReceivingLocations = results;
        });
        $scope.FromLocations = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.FromLocations = results;
        });
        $scope.Suppliers = [];
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIERWITHCITY', true).then(function (results) {
            $scope.Suppliers = results;
            $scope.FilteredSuppliers = results;
        });
        
        $scope.BundleRacks = restService.Get('ReferencelistService/BUNDLERACK', true).then(function (results) {
            $scope.BundleRacks = results;
        });

        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });

        $scope.Agents = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });
        $scope.Location = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
            if ($location.search().id == undefined) {
                $rootScope.BindCompany($scope, 'LREntry.lrcompanyid', $scope.CompanyChanged);
            }
        });

        $scope.Employee = restService.Get('ReferencelistService/PURCHASEMANAGER', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.LastLRInfo = null;
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }
        $scope.ErrorMessage = "";

        // model to be used to LREntry
        $scope.LREntryTemplate = {
            "id": 0,
            "lrno": null,
            "receiveddate": "",
            "lrdate": "",
            "lrentrydate": new Date(),
            "receivinglocationid": 0,
            "fromlocationid": 0,
            "supplierid": 0,
            "hasfreightcharge": false,
            "noofbundels": "",
            "noofpieces": "",
            "noofboxes": "",
            "weight": "",
            "issuedlrno": null,
            "issuedlrid": 0,
            "totalamount": "",
            "modifiedon": "",
            "createdon": new Date(),
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "isactive": true,
            "status": 0,
            "transportid": 0,
            "modeofdelivery": 2,
            lrentryfreightcharges: [],
            "attachment": [],
            invoiceno: "",
            retaillocationid: 0,
            retaillocationname: '',
            invoicedate: new Date(),
            cweight: ''
        };

        $scope.LREntry = (JSON.parse(JSON.stringify($scope.LREntryTemplate)));
        $scope.Slide = { 'title': '', 'image': '' };

        $scope.AddSlide = function () {
            if (typeof ($scope.LREntry.attachment) == 'undefined' || $scope.LREntry.attachment == null)
                $scope.LREntry.attachment = [];
            $scope.LREntry.attachment.push($scope.Slide);
            $scope.Slide = { 'title': '', 'image': '' };
        };

        $scope.LREntry.lrdate = new Date();
        $scope.LREntry.receiveddate = new Date();

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
                                $scope.LREntry.attachment.splice($scope.LREntry.attachment.indexOf(item), 1);
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

        $scope.UploadBill = function () {
            $.each($scope.currentimportdata.Items, function (k, v) {
                v.selected = false;
            });
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length == 0) {
                $rootScope.Toaster.Error("Please select the bills to be import");
                return;
            }
            var retList = [];
            $.each(seletedRows, function (k, v) {
                v.selected = true;
            });

            if ($scope.Slide.title == 'Import Quanto Bill')
                $scope.currentimportdata.autogeneratebarcode = true;

            if ($scope.LREntry != 'undefined' && $scope.LREntry != null)
                $scope.currentimportdata.lrentryid = $scope.LREntry.id;

            var request = {
                "Item": $scope.currentimportdata,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "File/UploadBillData",
                data: srequest
            }).success(function (result) {
                $scope.Slide.image = null;
                if (result.haserror) {
                    if (result.importcount > 0)
                        $rootScope.Toaster.Info(result.importcount + " Invoices Saved Successfully / With some errors " + result.errormessages);
                    else
                        $rootScope.Toaster.Error(result.errormessages);
                }
                else {
                    $rootScope.Toaster.Info(result.importcount + " Invoices Saved Successfully");
                }
            });
        }

        $scope.importbill = null;
        $scope.LoadQuantoBill = function () {
            $scope.importbill = null;
            if (typeof ($scope.LREntry.quantobillno) == 'undefined' || $scope.LREntry.quantobillno == null ||
                $scope.LREntry.quantobillno.length == '' || $scope.LREntry.quantobillno[0] == ' ') {
                $rootScope.Toaster.Info("Please enter the bill no");
                return;
            }
            request = {
                "Item": {
                    'billno': $scope.LREntry.quantobillno,
                    'isimport': true,
                    'removecancelled': true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.importbill = result[0];
                    $scope.importbill.supportuanno = true;
                    $scope.importbill.supportmrp = true;
                    $scope.importbill.gross = $scope.importbill.receivable - $scope.importbill.tax;

                    var company = $scope.Company.find(function (item) {
                        return item.id == result[0].companyid;
                    });
                    if (typeof (company) != 'undefined' && company != null && company.company_as_supplierid > 0) {
                        $scope.LREntry.supplierid = company.company_as_supplierid;
                    }

                    if ($scope.LREntry.supplierid > 0) {
                        var supliercompany = $scope.SupplierCompany.find(function (item) {
                            return item.supplierid == $scope.LREntry.supplierid;
                        });
                        if (typeof (supliercompany) != 'undefined' && supliercompany != null) {
                            $scope.importbill.suppliercompanyid = supliercompany.id;
                        }
                    }
                }
                else {
                    $rootScope.Toaster.Info("Given bill no " + $scope.LREntry.quantobillno + ' not found');
                }
            });
        }
        $scope.UploadQuantoBill = function () {
            if (!$scope.ValidateLREntry()) return;

            var request = {
                "Item": {
                    'bill': $scope.importbill, 'lrentry': $scope.LREntry,
                    'autogeneratebarcode': true,
                    'supportuanno': $scope.importbill.supportuanno,
                    'supportmrp': $scope.importbill.supportmrp,
                    'suppliercompanyid': $scope.importbill.suppliercompanyid,
                    'marginpercentage': $scope.importbill.marginpercentage,
                    'discountpercentage': $scope.importbill.discountpercentage,
                    'supportmarkdown': $scope.importbill.supportmarkdown
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrentryService/ImportQuantoBill",
                data: srequest
            }).success(function (result) {
                if (result.id > 0 && result.ErrorMessage == '') {
                    $rootScope.Toaster.Info("Quanto Bill Import Successfully completed. - " + result.lrentryno);
                    //$scope.LREntry = result;
                    $scope.LREntry = (JSON.parse(JSON.stringify($scope.LREntryTemplate)));
                }
                else {
                    if (result.ErrorMessage == '')
                        $rootScope.Toaster.Error("Failed to import bills. (" + result.ErrorMessage + ")");
                    else
                        $rootScope.Toaster.Error(result.ErrorMessage);
                }
            });
        }

        $scope.currentimportdata = null;
        $scope.fileuploadedcomplete = function (data) {
            $scope.Slide.image = null;
            $scope.currentimportdata = data;
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#importgrid').empty();
            }

            var igrid = new InfyGrid('importgrid', '80px', '100%', true, true, true, 370);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('suppliercompany', 'Supplier', 100);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('noofpieces', 'No of Pieces', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofrows', 'No of Rows', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('brandlist', 'Brand List', 100);
            igrid.AddFormatedColumn('billvalue', 'Bill Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('missing_attribute', 'Missing Attribute', 80, 'N0', 0);
            igrid.AddFormatedColumn('baseamount', 'Base Amount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax Amount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('missing_uan', 'Missing UAN', 80, 'N0', 0);
            var missinguan = $scope.currentimportdata.Items.find(function (item) {
                return item.missing_uan > 0;
            });
            if (missinguan && missinguan != null) {
                igrid.AddSimpleColumn('missing_uanlist', 'Missing UAN Items', 100);
            }

            igrid.AddFormatedColumn('missing_section', 'Missing Section', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_productcategory', 'Missing Product Category', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_product', 'Missing Product', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_brand', 'Missing Brand', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_size', 'Missing Size', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_pattern', 'Missing Pattern', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_material', 'Missing Material', 80, 'N0', 0);

            igrid.AddFormatedColumn('missing_style', 'Missing Style', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_colour', 'Missing Colour', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_type', 'Missing Type', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_design', 'Missing Design', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_fit', 'Missing Fit', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_sleeve', 'Missing Sleeve', 80, 'N0', 0);
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.currentimportdata.Items);
        }

        $scope.quantofileuploadedcomplete = function (data) {

            $scope.Slide.image = null;
            $scope.currentimportdata = data;
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#importgrid').empty();
            }

            var igrid = new InfyGrid('importgrid', '80px', '100%', true, true, true, 280);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('suppliercompany', 'Supplier Company', 100);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('noofpieces', 'No of Pieces', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billvalue', 'Bill Value', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('missing_attribute', 'Missing Attribute', 80, 'N0', 0);
            igrid.AddFormatedColumn('baseamount', 'Base Amount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax Amount', 80, 'N2', 2).total = true;


            igrid.AddFormatedColumn('missing_section', 'Missing Section', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_productcategory', 'Missing Productcategory', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_product', 'Missing Product', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_brand', 'Missing Brand', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_size', 'Missing Size', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_pattern', 'Missing Pattern', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_material', 'Missing Material', 80, 'N0', 0);

            igrid.AddFormatedColumn('missing_style', 'Missing Style', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_colour', 'Missing Colour', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_type', 'Missing Type', 80, 'N0', 0);
            igrid.AddFormatedColumn('missing_design', 'Missing Design', 80, 'N0', 0);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.currentimportdata.Items);
        }


        $scope.SearchCondition = {
            lrno: "",
            supplierid: "",
            lrentryno: "",
            fromdate: null,
            todate: null

        }
        $scope.LoadFrightCharges = function (list) {
            $scope.FrieghtChargeList = [];
            $.each(list, function (key, value) {
                var item = {
                    chargetypeid: value.id,
                    name: value.name,
                    charge: 0,
                    isactive: true
                };
                $scope.FrieghtChargeList.push(item);
            });
        };

        $scope.CompanyChanged = function () {
            var company = $scope.Company.find(function (item) {
                return item.id == $scope.LREntry.lrcompanyid;
            });
            if (typeof (company) != 'undefined' && company != null && company.internalbuyer) {
                $scope.LREntry.premargin = company.internalmargin;
            }

            if ($scope.isAttributefilter) {
                $scope.FilteredSuppliers = [];
                $.each($scope.Suppliers, function (key, value) {
                    if (value.companyid == 0 || value.companyid == $scope.LREntry.lrcompanyid)
                        $scope.FilteredSuppliers.push(value);
                });
            }
            $scope.ShowLastEntryNo();
        }

        $scope.Clear = function () {
            $scope.LREntry = (JSON.parse(JSON.stringify($scope.LREntryTemplate)));
            $.each($scope.FrieghtChargeList, function (key, value) {
                value.charge = 0;
                value.id = 0;
            });
        }

        $scope.CalculateDueDate = function () {
            if ($scope.Suppliers && $scope.Suppliers.length) {
                var supplier = $scope.Suppliers.find(function (item) {
                    return item.id == $scope.LREntry.supplierid;
                });
                if (supplier != null) {
                    if ($rootScope.GetNumber(supplier.creditdays) > 0) {
                        $scope.LREntry.duedate = $scope.LREntry.invoicedate.addDays(supplier.creditdays);
                        $scope.LREntry.stockholdingperiod = supplier.creditdays;
                    }
                }
            }
        };

        $scope.GetLastSupplier = function () {
            var supplier = $scope.FilteredSuppliers.find(function (item) {
                return item.id == $scope.LREntry.supplierid;
            });

            if (supplier &&
                supplier != null)
                $scope.LREntry.SupplierGSTNo = supplier.gstno;

            $scope.ShowSourceSupplier = false;
            var request = {
                "Item": {
                    "searchtype": 1,
                    "lrentryno": "",
                    "Request": [{ "key": "supplierid", "value": $scope.LREntry.supplierid }]
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "UtilityService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.Item != null &&
                    result.Item.Response != null && result.Item.Response.length >= 3) {
                    //$scope.UtilityItems.Response = result.Item.Response;
                    $.each(result.Item.Response, function (k, v) {
                        if (v.Key == "receivinglocationid") {
                            $scope.LREntry.receivinglocationid = parseInt(v.Value, 10);
                        }
                        else if (v.Key == "transportid") {
                            $scope.LREntry.transportid = parseInt(v.Value, 10);
                        }
                        else if (v.Key == "fromlocationid") {
                            $scope.LREntry.fromlocationid = parseInt(v.Value, 10);
                        }
                    });
                    $scope.CalculateDueDate();
                }
            }).error(function (result) {
            });

            var supplier = $scope.Suppliers.find(function (item) {
                return item.id == $scope.LREntry.supplierid;
            });
            if (typeof (supplier) != 'undefined' && supplier != null) {
                $scope.ShowSourceSupplier = supplier.supportsourcesupplier;
                $scope.LREntry.agentid = supplier.agentid;
                $scope.LREntry.SupplierGSTNo = supplier.gstno;
            }
            $scope.GetLastLRInfo();
        }

        $scope.SearchSupplierGST = function () {
            var supplier = $scope.Suppliers.find(function (sitem) {
                return sitem.gstno == $scope.LREntry.SupplierGSTNo;
            });
            if (supplier && supplier != null) {
                $scope.LREntry.supplierid = supplier.id;
                $scope.setFocusagentid();
            }
        }

        $scope.GetLastLRInfo = function () {
            $scope.CalculateDueDate();
            if ($scope.LREntry && $scope.LREntry.id && $scope.LREntry.id > 0) return;
            if ($scope.LREntry.supplierid > 0 && $scope.LREntry.supplierid > 0) {
                var request = {
                    "Item": {
                        "supplierid": $scope.LREntry.supplierid,
                        "lrno": $scope.LREntry.lrno,
                        "invoiceno": $scope.LREntry.invoiceno,
                        "invoicedate": $scope.LREntry.invoicedate,
                        "id": $scope.LREntry.id
                    },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "LrentryService/SearchLast",
                    data: srequest
                }).success(function (result) {
                    if (result != null && typeof (result.lrentryno) != 'undefined' && result.lrentryno != null) {
                        //if ($scope.LastLRInfo != null || $scope.LastLRInfo != 0) {
                        //    $rootScope.Toaster.Error("Entered LR with the Same InvoiceNo Already Entered By " + result.name);
                        //    return;
                        //}
                        $scope.LastLRInfo = result;
                        $rootScope.Toaster.Error("Entered LR with the Same InvoiceNo Already Entered By  - " + result.name);
                    }
                    else {
                        $scope.LastLRInfo = null;
                    }
                }).error(function (result) {
                });
            }
        }

        $scope.LastEntryNo = null;
        $scope.ShowLastEntryNo = function () {
            if ($scope.LREntry && $scope.LREntry.id && $scope.LREntry.id > 0) return;
            var entrylocationid = (typeof ($scope.LREntry.entrylocationid) && $scope.LREntry.entrylocationid > 0) ? $scope.LREntry.entrylocationid : 0;
            var lrcompanyid = (typeof ($scope.LREntry.lrcompanyid) && $scope.LREntry.lrcompanyid > 0) ? $scope.LREntry.lrcompanyid : 0;

            restService.Get('LrentryService/LastEntry/' + lrcompanyid + '/' + entrylocationid, true).then(function (results) {
                if (results == '')
                    $scope.LastEntryNo = '';
                else
                    $scope.LastEntryNo = 'Last Entry No :' + results;
            });
        }

        $scope.SaveOnly = function () {
            $scope.Save(false);
        }
        $scope.SaveAndNext = function () {
            $scope.Save(true);
        }

        $scope.WarehouseChanged = function () {
            $scope.LREntry.retaillocationid = 0;
            $scope.LREntry.retaillocationname = '';
            var warehouse = $scope.WarehouseLocations.find(function (item) {
                return item.id == $scope.LREntry.locationid;
            })

            if (typeof (warehouse) != 'undefined' && typeof (warehouse.additionalinfo) != 'undefined' && typeof (warehouse.additionalinfo.locationid) != 'undefined') {
                $scope.LREntry.retaillocationid = warehouse.additionalinfo.locationid;
                var retail = $scope.RetialLocations.find(function (item) {
                    return item.id == $scope.LREntry.retaillocationid;
                });
                $scope.LREntry.retaillocationname = typeof (retail) != 'undefined' && retail != null ? retail.name : '';
            }
        };

        $scope.ValidateLREntry = function () {
            $scope.LREntry.Lrentryfreightcharges = [];

            $.each($scope.FrieghtChargeList, function (k, v) {
                $scope.LREntry.Lrentryfreightcharges.push(v);
            });

            $scope.LREntry.isactive = true;

            if ($scope.isattachmentmandatory) {
                if (typeof ($scope.LREntry.attachment) == 'undefined' || $scope.LREntry.attachment == null || $scope.LREntry.attachment.length == 0) {
                    $rootScope.Toaster.Error("Please Attach Invoice copy.");
                    return false;
                }
            }

            if ($scope.LocationConfig.showwarehouse) {
                if (typeof ($scope.LREntry.locationid) == 'undefined' || $scope.LREntry.locationid == null || $scope.LREntry.locationid < 0) {
                    if ($scope.WarehouseLocations.length == 1) {
                        $scope.LREntry.locationid = $scope.WarehouseLocations[0].id;
                    }
                    else {
                        $rootScope.Toaster.Info("Please select Warehouse Location.");
                        return false;
                    }
                }
            }
            else {
                if ($scope.WarehouseLocations.length == 0
                    && $scope.LREntry.retaillocationid != 'undefined' && $scope.LREntry.retaillocationid != null && $scope.LREntry.retaillocationid > 0) {
                    $scope.LREntry.locationid = $scope.LREntry.retaillocationid;
                }
                else if ($scope.WarehouseLocations.length == 1) {
                    $scope.LREntry.locationid = $scope.WarehouseLocations[0].id;
                }
                else {
                    $rootScope.Toaster.Info("Multiple warehouse available, please select warehouse location.");
                    return false;
                }
            }

            if ($scope.warehousenotrequired) {
                if (typeof ($scope.LREntry.retaillocationid) == 'undefined' || $scope.LREntry.retaillocationid == null || $scope.LREntry.retaillocationid <= 0) {
                    if ($scope.RetialLocations != null && $scope.RetialLocations.length == 1) {
                        $rootScope.Toaster.Info("Warehouse support disabled, stock will be moved to store");
                        $scope.LREntry.retaillocationid = $scope.RetialLocations[0].id;
                    }
                    else {
                        $rootScope.Toaster.Error("Warehouse support disabled, Please select the auto retail location");
                        return false;
                    }
                }
            }

            if ($scope.showroomentryno) {
                if (typeof ($scope.LREntry.entrylocationid) == 'undefined' || $scope.LREntry.entrylocationid == null || $scope.LREntry.entrylocationid <= 0) {
                    $rootScope.Toaster.Error("Please select entry location ");
                    return false;
                }
            }

            if ($scope.showentrylocation) {
                if (typeof ($scope.LREntry.entrylocationid) == 'undefined' || $scope.LREntry.entrylocationid == null || $scope.LREntry.entrylocationid < 0) {
                    $rootScope.Toaster.Info("Please select Entry Location.");
                    return false;
                }
            }

            if ($scope.filterlocation && typeof ($scope.LREntry.lrcompanyid) != 'undefined' && $scope.LREntry.lrcompanyid > 0) {

                if (typeof ($scope.LREntry.locationid) != 'undefined' && $scope.LREntry.locationid > 0) {
                    var tlocation = $scope.Location.find(function (item) {
                        return item.id = $scope.LREntry.locationid;
                    });
                    if (tlocation.additionalinfo.parentid > 0 &&
                        tlocation.additionalinfo.parentid != $scope.LREntry.lrcompanyid) {
                        $rootScope.Toaster.Info("Entered location non releated with selected company.");
                        return false;
                    }
                }

                if (typeof ($scope.LREntry.entrylocationid) != 'undefined' && $scope.LREntry.entrylocationid > 0) {
                    var tlocation = $scope.Location.find(function (item) {
                        return item.id = $scope.LREntry.entrylocationid;
                    });
                    if (tlocation.additionalinfo.parentid > 0 &&
                        tlocation.additionalinfo.parentid != $scope.LREntry.lrcompanyid) {
                        $rootScope.Toaster.Info("Entry location non releated with selected company.");
                        return false;
                    }
                }

            }

            if ($scope.LREntry.lrno == null || $scope.LREntry.lrno == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please select LR Number.");
                return false;
            }
            if ($scope.LREntry.lrdate == null || $scope.LREntry.lrdate == 0) {
                $scope['setFocuslrdate']();
                $rootScope.Toaster.Info("Please select LR Date");
                return false;
            }
            if (($scope.LREntry.noofbundels == null || $scope.LREntry.noofbundels == '' || $scope.LREntry.noofbundels == 0)
                && ($scope.LREntry.noofboxes == null || $scope.LREntry.noofboxes == '' || $scope.LREntry.noofboxes == 0)
                && ($scope.LREntry.noofpieces == null || $scope.LREntry.noofpieces == '' || $scope.LREntry.noofpieces == 0)) {
                $scope['setFocusnoofbundels']();
                $rootScope.Toaster.Info("Please enter either No of Bundles/Box/Pieces.");
                return false;
            }

            if ($scope.LREntry.supplierid == null || $scope.LREntry.supplierid == 0) {
                $scope['setFocussupplierid']();
                $rootScope.Toaster.Info("Please select Supplier");
                return false;
            }
            if ($scope.LREntry.modeofdelivery == null || $scope.LREntry.modeofdelivery == 0) {
                $scope['setFocuslrmode']();
                $rootScope.Toaster.Info("Please select mode of delivery");
                return false;
            }

            if ($scope.LREntry.receiveddate == null || $scope.LREntry.receiveddate == 0) {
                $scope['setFocusreceiveddate']();
                $rootScope.Toaster.Info("Please select Received Date");
                return false;
            }

            //if ($scope.LREntry.locationid != null && $scope.LREntry.locationid > 0 &&
            //    $scope.LREntry.retaillocationid != null && $scope.LREntry.retaillocationid > 0) {
            //    var fromlocation = $scope.Location.find(function (item) { return item.id == $scope.LREntry.locationid; });
            //    var tolocation = $scope.Location.find(function (item) { return item.id == $scope.LREntry.retaillocationid; });
            //    if (fromlocation != null && tolocation != null && fromlocation.additionalinfo != null && tolocation.additionalinfo != null &&
            //        fromlocation.additionalinfo.parentid > 0 && tolocation.additionalinfo.parentid > 0 &&
            //        fromlocation.additionalinfo.parentid != tolocation.additionalinfo.parentid) {
            //        $scope['setFocuslocationid']();
            //        $rootScope.Toaster.Error("Warehouse company and retail company are difference, can't do auto transfer");
            //        return false;
            //    }
            //}
            return true;
        }

        $scope.DirectSave = function (moveNext) {

            if ($scope.SaveRequest)
                return;

            request = {
                "Item": $scope.LREntry,
                "SecurityToken": "String content"
            }
            $scope.SaveRequest = true;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrentryService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    if (moveNext) {
                        $rootScope.Toaster.Info("LR Saved Successfully.! With Entry No :" + result.lrentryno);
                        $location.url('/Invoice?lrid=' + result.id);
                    }
                    else {
                        /* $scope.ErrorMessage = "Successfully saved";
                         $scope.ErrorStyle = "success";*/
                        $rootScope.Toaster.Info("LR Saved Successfully.! With  With Entry No :" + result.lrentryno);
                        $scope.LREntry = result;

                        $scope.LastEntryNo = result.lrentryno;
                        $scope.LastEntryBundles = result.noofbundels;
                        $scope.LREntry = (JSON.parse(JSON.stringify($scope.LREntryTemplate)));
                    }
                }
                else {
                    /*$scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";*/
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }
        $scope.Save = function (moveNext) {

            if (!$scope.ValidateLREntry()) return;

            if ($scope.LREntry.id && $scope.LREntry.id > 0) {

                var modifiedList = [];
                if ($scope.LREntry.OLD_lrcompanyid != $scope.LREntry.lrcompanyid)
                    modifiedList.push('Company Changed.');
                if ($scope.LREntry.OLD_supplierid != $scope.LREntry.supplierid)
                    modifiedList.push('Supplier Changed.');
                if ($scope.LREntry.OLD_retaillocationid != $scope.LREntry.retaillocationid)
                    modifiedList.push('Retail Location Changed.');

                if (modifiedList.length > 0) {

                    $.confirm({
                        title: 'Modoified',
                        content: 'Do you want to Proceed?<br>' + modifiedList.join('<br>'),
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

                                    if ($scope.LREntry.OLD_lrcompanyid != $scope.LREntry.lrcompanyid)
                                        $scope.LREntry.Changed_lrcompanyid = true;
                                    if ($scope.LREntry.OLD_supplierid != $scope.LREntry.supplierid)
                                        $scope.LREntry.Changed_supplierid = true;
                                    if ($scope.LREntry.OLD_retaillocationid != $scope.LREntry.retaillocationid)
                                        $scope.LREntry.Changed_retaillocationid = true;

                                    $scope.DirectSave(moveNext);
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
                else {
                    $scope.DirectSave(moveNext);
                }

            }
            else {
                $scope.DirectSave(moveNext);
            }
        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete LR - no ' + item.lrno + "?",
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
                                    url: "LrentryService/Delete",
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
                $scope.LREntry = item;
                var supplier = $scope.Suppliers.find(function (item) {
                    return item.id == $scope.LREntry.supplierid;
                });
                if (typeof (supplier) != 'undefined' && supplier != null) {
                    $scope.LREntry.SupplierGSTNo = supplier.gstno;
                }
                $scope.LREntry.OLD_lrcompanyid = $scope.LREntry.lrcompanyid;
                $scope.LREntry.OLD_supplierid = $scope.LREntry.supplierid;
                $scope.LREntry.OLD_retaillocationid = $scope.LREntry.retaillocationid;

                request = {
                    "Item": { id: item.id },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "LrentryService/Search",
                    data: srequest
                }).success(function (result) {
                    if ($scope.FrieghtChargeList != null && angular.isArray($scope.FrieghtChargeList)) {
                        $.each($scope.FrieghtChargeList, function (key, value) {
                            var storeditem = result[0].Lrentryfreightcharges.find(function (fcharge) {
                                return fcharge.chargetypeid == value.chargetypeid;
                            })
                            if (typeof (storeditem) != 'undefined' && storeditem != null) {
                                value.id = storeditem.id;
                                value.charge = storeditem.charge;
                            }
                            else {
                                value.charge = 0;
                                value.id = 0;
                            }
                        });
                    }

                }).error(function (result) {
                });
            } else if (action == 'Remove') {
                item.charge = 0;
            }
            else if (action == 'Print Sticker') {
                $rootScope.Prompt('LR Bundle Sticker', 'No Of Copies', function (count) {
                    var requestitem = { 'copy': count, 'id': item.id };
                    $scope.ChoseReport(requestitem);
                });
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'LR ENTRY DETAILS'

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


        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrentryService/Search",
                data: srequest
            }).success(function (result) {
                //alert(result.length);
                $scope.SearchResult = result;
                if ($scope.SearchCondition && $scope.SearchCondition.modeofdelivery &&
                    $scope.SearchCondition.modeofdelivery == 10) {
                    $scope.LRRCMConfig();
                }
                else {
                    $scope.LREntryConfig();
                }
            }).error(function (result) {
            });
        }

        $scope.LRRCMConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);
            igrid.AddSimpleColumn('lrentryno', 'LR Entry No', 100);
            igrid.AddFormatedColumn('lrentrydate', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('lrno', 'LR No', 100);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier', 140, $scope.Suppliers);
            igrid.AddReferenceColumn('transportid', 'Transport', 120, $scope.Transports);
            igrid.AddSimpleColumn('tgst', 'GST No', 100);
            igrid.AddFormatedColumn('freightcharge', 'Taxable', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxpercentage', 'Tax %', 100, 'N2', 2);

            column = igrid.AddCustomColumn('IGST', 'IGST', 100, function (item, col) {
                if (item.isinterstate) {
                    return $filter('IndianNumber')(item.tax, 2);
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.isinterstate) {
                    return item.tax;
                }
                return 0;
            }


            column = igrid.AddCustomColumn('CGST', 'CGST', 100, function (item, col) {
                if (!item.isinterstate && item.tax != 0) {
                    return $filter('IndianNumber')(item.tax/2, 2);
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (!item.isinterstate && item.tax != 0) {
                    return item.tax/2;
                }
                return 0;
            }


            column = igrid.AddCustomColumn('SGST', 'SGST', 100, function (item, col) {
                if (!item.isinterstate && item.tax!=0) {
                    return $filter('IndianNumber')(item.tax / 2, 2);
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (!item.isinterstate && item.tax != 0) {
                    return item.tax / 2;
                }
                return 0;
            }

            igrid.AddFormatedColumn('total', 'Total Amount', 100, 'N2', 2).total = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');
        }

        $scope.LREntryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddSimpleColumn('lrentryno', 'LR Entry No', 100);
            igrid.AddFormatedColumn('lrentrydate', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('lrno', 'LR No', 100);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receiveddate', 'Received On', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('fromlocationid', 'From Location', 120, $scope.FromLocations);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 140, $scope.Suppliers);
            igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.Agents);
            igrid.AddReferenceColumn('transportid', 'Transport', 120, $scope.Transports);
            igrid.AddSimpleColumn('noofbundels', 'Bundles', 50);
            igrid.AddSimpleColumn('noofpieces', 'Pieces', 50);
            igrid.AddFormatedColumn('frightcharge', 'Fright', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'Total Amount', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('statusname', 'Status', 100);
            igrid.AddSimpleColumn('createduser', 'Created By', 80);
            igrid.AddFormatedColumn('createdon', 'Created On', 100, 'dd-MM-yyyy hh:mm');
            igrid.AddSimpleColumn('modifieduser', 'Modified By', 80);
            igrid.AddFormatedColumn('modifiedon', 'Modified On', 100, 'dd-MM-yyyy hh:mm');

            var commands = "";
            if ($rootScope.UserInfo.p_LRENTRY && $rootScope.UserInfo.p_LRENTRY != null && $rootScope.UserInfo.p_LRENTRY.print)
                commands += "<input type='button' class='btn btn-green btn-xs' value='Print Sticker'/>&nbsp;"
            if ($rootScope.UserInfo.p_LRENTRY && $rootScope.UserInfo.p_LRENTRY != null && $rootScope.UserInfo.p_LRENTRY.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_LRENTRY && $rootScope.UserInfo.p_LRENTRY != null && $rootScope.UserInfo.p_LRENTRY.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"

            if ($rootScope.UserInfo.p_LRENTRY && $rootScope.UserInfo.p_LRENTRY != null
                && ($rootScope.UserInfo.p_LRENTRY.modify || $rootScope.UserInfo.p_LRENTRY.delete || $rootScope.UserInfo.p_LRENTRY.print)) {
                igrid.AddButtonColumn('action', 'ACTION', 180, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Print Sticker')
                            $scope.ManageSearchItem('Print Sticker', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');

        }

        $scope.locationidfocus = function () {
            if ($scope.LocationConfig.showwarehouse) {
                $scope.setFocuslocationid()
            }
            else if ($scope.LocationConfig.showentrylocation) {
                $scope.setFocuselocationid();
            }
            else {
                $scope.setFocusRlocationid();
            }
        }


        $scope.ptuploadedcomplete = function (value) {
            $scope.UploadedExcel = value;
            $scope.uploadfile = null;
            $scope.Upload(value);
        }

        $scope.Upload = function (excel) {
            var dialog = ngDialog.open({
                template: 'ImportItem',
                data: {
                    'Excel': excel,
                    'scope': $scope
                },
                width: '600px',
                height: '370px',
                controller: ['$scope', '$timeout', 'localStorageService',
                    function ($scope, $timeout, localStorageService) {
                        $scope.ParentScope = $scope.ngDialogData.scope;
                        $scope.ExcelColumns = [];
                        $scope.sheetname = $scope.ngDialogData.Excel.sheets[0].name;
                        $.each($scope.ngDialogData.Excel.sheets[0].columns, function (k, v) {
                            $scope.ExcelColumns.push({ 'id': v, 'name': v });
                        });

                        $timeout(function () {
                            var columns = localStorageService.get('MigrationData_LREntry');
                            if (typeof (columns) != 'undefined' && columns != null && columns.length > 0) {
                                $.each($scope.Columns, function (k, v) {

                                    var eColumn = columns.find(function (item) {
                                        return item.name == v.name;
                                    });

                                    if (typeof (eColumn) != 'undefined' && eColumn != null) {
                                        v.excelcolunmn = eColumn.excelcolunmn;
                                    }

                                });
                            }
                        }, 50);


                        $scope.Columns = [
                            { table: 'Purchase', display: 'Bill No', name: 'Bill No', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Bill Date', name: 'Bill Date', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'DesignID', name: 'DesignID', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'LR Date', name: 'LR Date', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Purchase Discount (Total Qty)', name: 'Purchase Discount', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'From Location', name: 'From Location', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Supplier Name', name: 'Supplier Name', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Source Supplier Name', name: 'Source Supplier Name', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Company', name: 'Company', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Location', name: 'Location', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Supplier GST', name: 'Supplier GST', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'LRNO', name: 'LRNO', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Qty', name: 'Qty', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'MRP', name: 'MRP', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Cost Price - (Inclusive Discount)', name: 'Cost Price', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Tax Percentage', name: 'Tax Percentage', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Tax Peritem', name: 'Tax Peritem', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Price (Before Discount)', name: 'Price (Before Discount)', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Sales Discount %', name: 'Sales Discount %', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Final Price', name: 'Final Price', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'UAN', name: 'UAN', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'HSN', name: 'HSN', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Item Code', name: 'Item Code', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Section', name: 'Section', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Product Category', name: 'Product Category', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Product', name: 'Product', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Brand', name: 'Brand', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Size', name: 'Size', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Style', name: 'Style', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Pattern', name: 'Pattern', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Type', name: 'Type', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Material', name: 'Material', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Colour', name: 'Colour', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Fit', name: 'Fit', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Sleeve', name: 'Sleeve', isnumber: 'N', isdate: 'N' },
                            { table: 'Purchase', display: 'Full Name', name: 'Full Name', isnumber: 'N', isdate: 'N' }];

                        $scope.CreateBillImport = function () {
                            if ($scope.BillImportName && $scope.BillImportName != null &&
                                $scope.BillImportName != '') {
                                var excelcolumns = [];
                                $.each($scope.Columns, function (k, v) {
                                    if (typeof (v.excelcolunmn) == 'undefined' || v.excelcolunmn == null)
                                        excelcolumns.push('');
                                    else
                                        excelcolumns.push(v.excelcolunmn);
                                })
                                var request = {
                                    "Item": {
                                        'type': 'BILLIMPORT', 'name': $scope.BillImportName,
                                        'additionalinfo': { 'formula': excelcolumns.join(',') }
                                    },
                                    "securitytoken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "ReferencelistService/Save",
                                    data: srequest
                                }).success(function (result) {

                                    if (result != null && result.id > 0) {
                                        $scope.closeThisDialog(result);
                                    }

                                }).error(function (result) {
                                });

                            }
                        }

                        $scope.Selected = function () {

                            localStorageService.set('MigrationData_LREntry', $scope.Columns);

                            var request = {
                                'columns': $scope.Columns,
                                'tablename': 'Items',
                                'sheetname': $scope.sheetname,
                                'uploadfileid': $scope.ngDialogData.Excel.fileid
                            };

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "LrentryService/MigrateData",
                                data: srequest
                            }).success(function (result) {
                                $scope.closeThisDialog({ 'message': "Data uploaded successfully",'data':result});
                            }).error(function (result) {
                            });
                        };

                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document'
                        || data.value == '$escape')
                        return;

                    $scope.fileuploadedcomplete(data.value.data);
                    //if (data.message) {
                    //    $rootScope.Toaster.Info("Data uploaded successfully");
                    //}
                    //else {
                    //    $scope.BillImports.splice(0, 0, data);
                    //}
                }
            });
        }

        $scope.entrylocationidfocus = function () {
            if ($scope.LocationConfig.showentrylocation) {
                $scope.setFocuselocationid();
            }
            else {
                $scope.setFocusRlocationid();
            }
        }

        $scope.LoadLREntry = function () {
            if (typeof ($scope.Suppliers) == 'undefined' || $scope.Suppliers == null
                || $scope.Suppliers.length == 0) {
                $timeout(function () {
                    $scope.LoadLREntry();
                }, 50);
                return;
            }

            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "LrentryService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length == 1) {
                        $scope.LREntry = result[0];
                        var supplier = $scope.Suppliers.find(function (item) {
                            return item.id == $scope.LREntry.supplierid;
                        });
                        if (typeof (supplier) != 'undefined' && supplier != null) {
                            $scope.LREntry.SupplierGSTNo = supplier.gstno;
                            $scope.ShowSourceSupplier = supplier.supportsourcesupplier;
                        }
                        $scope.LREntry.OLD_lrcompanyid = $scope.LREntry.lrcompanyid;
                        $scope.LREntry.OLD_supplierid = $scope.LREntry.supplierid;
                        $scope.LREntry.OLD_retaillocationid = $scope.LREntry.retaillocationid;
                    }
                }).error(function (result) {
                });
            }
        }

        $timeout(function () {
            $scope.LoadLREntry();
        }, 200);


        $scope.PrintBarcode = function () {
            
            if ($scope.LREntry.id == 'undefined' || $scope.LREntry.id == null || $scope.LREntry.id == 0) {
                $rootScope.ShowAlert('LR Bundle', 'Please select LR');
                return;
            }
            $rootScope.Prompt('LR Bundle Sticker', 'No Of Copies', function (count) {
                var requestitem = { 'copy': count, 'id': $scope.LREntry.id };
                $scope.ChoseReport(requestitem);
            });
        };

        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('LRBUNDLELABEL', function (printdata) {
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
                    url: "LrentryService/PrintBarcode",
                    data: srequest
                }).success(function (result) {
                    if (result != null && result.length > 0) {
                        $rootScope.Sent2Printer(result.join(';'), printdata);
                        return;
                    }
                });
            });
        }


        $scope.AgentChanged = function () {
            var agent = $scope.Agents.find(function (item) {
                return item.id == $scope.LREntry.agentid;
            });
            if (agent && agent != null) {
                $scope.LREntry.agentcommission = agent.commissionpercentage;
            }
        }


        $scope.LoadAppSetting = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null ||
                $scope.AllowedRetialLocations == undefined || $scope.AllowedRetialLocations == null) {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 50);
                return;
            }
            $scope.system_showlgstonlrentry = false ||$rootScope.UserInfo.appsettings.system_showlgstonlrentry;
            $scope.LocationConfig.showwarehouse = false ||$rootScope.UserInfo.appsettings.system_multiplewarehouse;
            $scope.LocationConfig.showentrylocation = false ||$rootScope.UserInfo.appsettings.system_showroompurchase;
            $scope.LocationConfig.label = $rootScope.UserInfo.appsettings.system_showroompurchase ? 'Auto transfer Location' : 'Auto transfer Location';
            $scope.LocationConfig.filterlocation = false ||$rootScope.UserInfo.appsettings.system_supportcompanylocationfilter;
            $scope.isattachmentmandatory = false ||$rootScope.UserInfo.appsettings.system_supportinvoiceattachments;
            $scope.isAttributefilter = false ||$rootScope.UserInfo.appsettings.system_supportattributefilter;
            $scope.warehousenotrequired = false ||$rootScope.UserInfo.appsettings.system_warehousenotrequired;
            $scope.showroomentryno = false ||$rootScope.UserInfo.appsettings.system_showroomentryno;
            $scope.disablelrentrydate = false || $rootScope.UserInfo.appsettings.system_disablelrentrydate;
            $scope.disableautotransferpermission = false || $rootScope.UserInfo.appsettings.system_disableautotransferpermission;

            if ($scope.disableautotransferpermission) {
                restService.Get('ReferencelistService/MYRETAILLOCATION', true).then(function (results) {
                    results.push({ 'id': 0, 'name': 'NONE' });
                    $scope.AllowedRetialLocations = results;
                });
            }

            if ($scope.system_showlgstonlrentry) {
                $scope.SupplierClass = 'col-md-5';
            }
        }
        
        $timeout(function () {
            $scope.LoadAppSetting();
            $scope.setFocusfirst();
        }, 10);

    }]);
