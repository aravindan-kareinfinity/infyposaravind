
var app = angular.module('InfinityLab');

app.controller('NumberConfigurationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Location = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.VoucherTypes = [
            { 'id': 1, 'name': 'Payment' },
            { 'id': 2, 'name': 'Receipt' },
            { 'id': 3, 'name': 'Purchase' },
            { 'id': 4, 'name': 'Sales' },
            { 'id': 5, 'name': 'Credit Note' },
            { 'id': 6, 'name': 'Debit Note' },
            { 'id': 7, 'name': 'Contra' },
            { 'id': 8, 'name': 'Journal Cr' },
            { 'id': 16, 'name': 'Journal Dr' },
            { 'id': 14, 'name': 'Advance Payment' },
            { 'id': 15, 'name': 'Petty Cash Expense' },
            { 'id': 33, 'name': 'Purchase Order' },
            { 'id': 0, 'name': 'All Other Vouchers' }
        ];

        $scope.NumberType = [{ 'id': 'lrinvoicereturn.returnno', 'name': 'Debit Note' },
            { 'id': 'bill.billno', 'name': 'Bill No' },
            { 'id': 'gstbill.billno', 'name': 'GST - Bill No' },
            { 'id': 'bill.deliveryno', 'name': 'Delivery No' },
            { 'id': 'delivery.altationno', 'name': 'Altation No' },
            { 'id': 'lrreceived.receiptno', 'name': 'LR Received Receipt No' },
            { 'id': 'billreturn.billno', 'name': 'Return Bill' },
            { 'id': 'lrissue.issueno', 'name': 'LR Issue' },
            { 'id': 'employee.employeecode', 'name': 'Employee Code' },
            { 'id': 'billsettlementmaster.code', 'name': 'Settlement Master Code' },
            { 'id': 'bundles.code', 'name': 'Bundle Code' },
            { 'id': 'lrinvoice.invoiceentryno', 'name': 'LR Invoice Entry No' },
            { 'id': 'lrentry.lrentryno', 'name': 'LR Entry No' },
            { 'id': 'lrentry.lrexpenseno', 'name': 'LR Expense No' },
            { 'id': 'lrentry.lrintertransfer', 'name': 'LR Intertransfer No' },
            { 'id': 'lrinvoicereturn.excessnote', 'name': 'Supplier Excess Note' },
            { 'id': 'lrinvoicereturn.creditnote', 'name': 'Supplier Credit Note' },
            { 'id': 'lrinvoicereturn.advancenote', 'name': 'Supplier Advance Note' },
            { 'id': 'lrinvoicereturn.draft', 'name': 'Supplier Dr/Cr Draft' },
            { 'id': 'bommaster.bommastercode', 'name': 'BOM Master Code' },
            { 'id': 'invoice.invoiceentryno', 'name': 'BOM Invoice No' },
            { 'id': 'agentpayment.receiptno', 'name': 'Agent Payment Receipt No' },
            { 'id': 'lrinvoice.designid', 'name': 'Design ID' },
            { 'id': 'tailoringorder.orderno', 'name': 'Tailoring Order No' },
            { 'id': 'tailoringorder.outwardno', 'name': 'Tailoring Outward No' },
            { 'id': 'tailoringorder.inwardno', 'name': 'Tailoring Inward No' },
            { 'id': 'jobworkbill.invoiceno', 'name': 'Job Work Order No' },
            { 'id': 'jobworkbillgrn.entryno', 'name': 'Job Work GRN No' },
            { 'id': 'mfgsampleorder.entryno', 'name': 'MFG - Sample Order No' },
            { 'id': 'mfgworkorder.workorderno', 'name': 'MFG - Job Work Order No' },
            { 'id': 'mfgjoborder.joborderno', 'name': 'MFG - Job Work Order No' },
            { 'id': 'mfgworkordergrn.entryno', 'name': 'MFG - Job Work Order GRN No' },
            { 'id': 'bominvoiceitem.bominwardcode', 'name': 'BOM Inward Barcode' },
            { 'id': 'voucher.voucherno', 'name': 'Voucher No' },
            { 'id': 'bulkbill.billno', 'name': 'Bulk Bill No' },
            { 'id': 'bulkbill.ebillno', 'name': 'Estimate Bill No' },
            { 'id': 'splitbarcode.code', 'name': 'Split Barcode Bundle Code' },
            { 'id': 'stock.packserial', 'name': 'Barcode PACK/SERIAL' },
            { 'id': 'stock.barcode62', 'name': 'Barcode Alphanumeric' },
            { 'id': 'billmaster.code', 'name': 'Bill Master Code' },
            { 'id': 'customer.code', 'name': 'Customer Code' },
            { 'id': 'customer.loyeltyno', 'name': 'Customer Loyelty Card No' },
            { 'id': 'giftvoucherseq.code', 'name': 'Gift Voucher SeqNo' },
            { 'id': 'customerorder.customerorderno', 'name': 'Customer Order No' },
            { 'id': 'products.code', 'name': 'Product Code' },
            { 'id': 'stock.barcode36', 'name': 'Barcode Number & Letter' },
            { 'id': 'stock.pack10', 'name': 'Pack Barcode' },
            { 'id': 'brand.code', 'name': 'Brand Code' },
            { 'id': 'STATE.code', 'name': 'State Code' },
            { 'id': 'COLOUR.code', 'name': 'Colour Code' },
            { 'id': 'STYLE.code', 'name': 'Style Code' },
            { 'id': 'CITY.code', 'name': 'City Code' },
            { 'id': 'PATTERN.code', 'name': 'Pattern Code' },
            { 'id': 'DIVISION.code', 'name': 'Section Code' },
            { 'id': 'TYPE.code', 'name': 'Type Code' },
            { 'id': 'SIZE.code', 'name': 'Size Code' },
            { 'id': 'MATERIAL.code', 'name': 'Material Code' },
            { 'id': 'UAN.code', 'name': 'UAN/G1 Code' },
            { 'id': 'Mfgbinstorage.UniqueCode', 'name': 'Bin Number' },
            { 'id': 'purchaseorder.pono', 'name': 'Purchase Order Number' },
            { 'id': 'salesorder.sono', 'name': 'Sales Order Number' },
            { 'id': 'supplier.code', 'name': 'Supplier Code' },
            { 'id': 'stock.barcode10', 'name': 'Barcode - Numbers' },
            { 'id': 'sample.receiptno', 'name': 'Samples - No' },
            { 'id': 'folder.receiptno', 'name': 'Folder - No' },
            { 'id': 'packingslip.packingno', 'name': 'Packing Slip - No' },
            { 'id': 'componentpo.pono', 'name': 'Component PO Number' },
            { 'id': 'mfgdesign.designcode', 'name': 'Mfg - Design code' },
            { 'id': 'mfgdesign.designitemcode', 'name': 'Mfg - Design item code' },
            { 'id': 'items.businesscode', 'name': 'Item Business Code' },
            { 'id': 'mfgdesign.uid', 'name': 'Mfg - UAN sequence' },
            { 'id': 'mfgdesign.autonumber', 'name': 'Mfg - Colour/UAN Auto Number' },
            { 'id': 'billcreditnote.creditnoteno', 'name': 'Customer Credit Note No' },
            { 'id': 'supplierpayment.receiptno', 'name': 'Supplier Payment Receipt No' },
            { 'id': 'transportpayment.receiptno', 'name': 'Transport Payment Receipt No' },
            { 'id': 'SupplierTradeAgreement.itemcode', 'name': 'Vendor Agreement Code' },
            { 'id': 'stockdumping.dumpcode', 'name': 'Stock Dumping Code' }
        ];
        
        $scope.Entity = {
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "description": "",
            "prefix": "",
            "suffix": "",
            "currentidentity": 0,
            "filler": "",
            "nolength": 6,
            "type": "",
            "typeid": 0,
            "companyid": 0,
            "locationid": 0
        };

        $scope.AutoNumber = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.AutoNumber = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";
        
        $scope.SearchCondition = {
            numbertypeid: 0,
            locationid: 0,
            companyid:0
        }
        $scope.New = function () {
            $scope.AutoNumber = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.SearchNumberType = function (typestr, list) {
            var found = list.find(function (item) {
                return item.id == typestr;
            })
            if (found != 'undefined' && found != null)
                return found.name;
            return '';
        }
        //SearchLocation(item.locationid, item.companyid)
        $scope.SearchLocation = function (locationid, companyid) {
            var l = $scope.SearchRef(locationid, $scope.Location, null);
            var c = $scope.SearchRef(companyid, $scope.Company, null);
            if (c == null) {
                if (l == null)
                    return 'All Location';
                else
                    return l;
            }
            else {
                if (l == null)
                    return c;
                else
                    return c + '(' + l + ')';
            }
        }

        $scope.IsWarehouse = function (locationid) {
            var warehouse = false;
            $.each($scope.Location, function (k, v) {
                if (v.id == locationid) {
                    var additionalinfo = v.additionalinfo;
                    if (additionalinfo != null)
                        warehouse = additionalinfo.iswarehouse;
                }
            });
            return warehouse;
        }
        $scope.Search = function () {
            //var numbertype = '';
            //var found = $scope.NumberType.find(function (item) {
            //    return item.id == $scope.SearchCondition.numbertypeid;
            //})
            //if (found != 'undefined' && found != null)
            //    numbertype = found.id;

            var locationid = $scope.IsWarehouse($scope.SearchCondition.locationid) == true ? 0 : $scope.SearchCondition.locationid;
            request = {
                "Item": {
                    type: $scope.SearchCondition.type,
                    locationid: locationid,
                    iswarehouse: $scope.IsWarehouse($scope.SearchCondition.locationid),
                    'companyid': $scope.SearchCondition.companyid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AutonumberService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }
        $scope.Save = function () {

            if ($scope.AutoNumber.type == null || $scope.AutoNumber.type == '') {
                $scope['setFocusAutoNumber']();
                $rootScope.Toaster.Error("Please Select Number Type");
                return;
            }
            if ($scope.AutoNumber.companyid == null || $scope.AutoNumber.companyid == '') {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Error("Please Select the Company");
                return;
            }
            //if ($scope.AutoNumber.locationid == null || $scope.AutoNumber.locationid == '') {
            //    $scope['setFocuslocationid']();
            //    $rootScope.Toaster.Error("Please Select the Location");
            //    return;
            //}

            var found = $scope.NumberType.find(function (item) {
                return item.id == $scope.AutoNumber.type;
            })

            if (found != 'undefined' && found != null) {
                $scope.AutoNumber.type = found.id;
                $scope.AutoNumber.description = found.name;
            }

            $scope.AutoNumber.locationid = $scope.IsWarehouse($scope.AutoNumber.locationid) == true ? 0 : $scope.AutoNumber.locationid;

            var request = {
                "Item": $scope.AutoNumber,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AutonumberService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Number Configuration Saved Successfully.!");
                    $scope.AutoNumber = (JSON.parse(JSON.stringify($scope.Entity)));
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
                                    url: "AutonumberService/Delete",
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
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.AutoNumber = item;
                //var found = $scope.NumberType.find(function (item) {
                //    return item.code == $scope.AutoNumber.type;
                //})
                //if (found != 'undefined' && found != null)
                //    $scope.AutoNumber.typeid = found.id;
            }
        }


        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);