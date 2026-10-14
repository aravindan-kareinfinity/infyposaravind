
var app = angular.module('InfinityLab');

app.controller('DataMigrationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout','localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, localStorageService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.OrganizationSelected = false;
        $scope.SearchOrganization = function () {
            $scope.OrganizationSelected = false;
            $scope.OrganizationName = "";
            restService.Get('OrganisationService/Search/' + $scope.organizationcode, true).then(function (results) {
                $scope.CurrentOrganization = results;
                if ($scope.CurrentOrganization != null) {
                    $scope.OrganizationSelected = true;
                    $scope.OrganizationName = $scope.CurrentOrganization.name;
                }
            });
        }

        $scope.TableColumns = [
            //{ table: 'Tax', display: 'Code', name: 'code', isnumber: 'N', isdate: 'N' },
            //{ table: 'Tax', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },
            //{ table: 'Tax', display: 'Tax Percentage', name: 'percentage', isnumber: 'Y', isdate: 'N' },

            { table: 'Product', display: 'Code', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Product', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Product', display: 'Selling Mode', name: 'sellingmode', isnumber: 'N', isdate: 'N' },
            { table: 'Product', display: 'Purchase Tax Percentage', name: 'purchasetax', isnumber: 'Y', isdate: 'N' },
            { table: 'Product', display: 'Sale Tax Percentage', name: 'saletax', isnumber: 'Y', isdate: 'N' },
            { table: 'Product', display: 'HSN Code', name: 'hsncode', isnumber: 'N', isdate: 'N' },
            { table: 'Product', display: 'Section', name: 'section', isnumber: 'N', isdate: 'N' },
            { table: 'Product', display: 'Product Group', name: 'productgroup', isnumber: 'N', isdate: 'N' },

            { table: 'Brand', display: 'Code', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Brand', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },

            { table: 'Supplier', display: 'Code', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Supplier Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Supplier Company', name: 'SupplierCompany.name', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Contact Person', name: 'contactperson', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Contact No 1', name: 'contactnumber', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Contact No 2', name: 'SupplierCompany.companycontactno', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Address', name: 'SupplierCompany.address', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'City', name: 'SupplierCompany.cityid', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'State', name: 'SupplierCompany.stateid', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Country', name: 'SupplierCompany.countryid', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Pincode', name: 'SupplierCompany.pincode', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Supplier Email ID', name: 'emailid', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Credit Days', name: 'creditdays', isnumber: 'Y', isdate: 'N' },
            { table: 'Supplier', display: 'URD(Un Registred)', name: 'isunregistered', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Support PO', name: 'supportpo', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Agent Name', name: 'SupplierCompany.agentid', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'GST', name: 'SupplierCompany.tinanddate', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Interstate', name: 'SupplierCompany.isinterstatesale', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'TAN', name: 'SupplierCompany.tan', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'PAN', name: 'SupplierCompany.pan', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'MSME NO', name: 'SupplierCompany.msmeno', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Bank Name', name: 'SupplierCompany.bankid', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Branch Name', name: 'SupplierCompany.bankbranch', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Bank Account Name', name: 'SupplierCompany.bankaccountname', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Bank Account No', name: 'SupplierCompany.bankaccountno', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Bank IFSC', name: 'SupplierCompany.bankifci', isnumber: 'N', isdate: 'N' },
            { table: 'Supplier', display: 'Created On', name: 'activatedon', isnumber: 'N', isdate: 'Y' },
            { table: 'Supplier', display: 'Agent Purchase', name: 'supportsourcesupplier', isnumber: 'N', isdate: 'Y' },

            { table: 'Agent', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Contact Person', name: 'contactperson', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Contact No', name: 'contactno', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Email ID', name: 'emailid', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Address', name: 'address', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'PAN', name: 'pan', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'GST', name: 'gst', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Commission Amt', name: 'commissionamount', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Commission%', name: 'commissionpercentage', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'City', name: 'cityid', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'State/Country', name: 'stateid', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Pincode', name: 'pincode', isnumber: 'N', isdate: 'N' },
            { table: 'Agent', display: 'Country', name: 'countryid', isnumber: 'N', isdate: 'N' },

            { table: 'Design', display: 'Code', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'Product', name: 'product', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'Sleeve', name: 'sleeve', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'Fabric', name: 'fabric', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'Year', name: 'year', isnumber: 'Y', isdate: 'N' },
            { table: 'Design', display: 'Month', name: 'month', isnumber: 'Y', isdate: 'N' },
            { table: 'Design', display: 'Size Code', name: 'sizecode', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'Colour', name: 'colour', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'UAN', name: 'uan', isnumber: 'N', isdate: 'N' },
            { table: 'Design', display: 'Cost', name: 'cost', isnumber: 'Y', isdate: 'N' },
            { table: 'Design', display: 'B2B Rate', name: 'b2b', isnumber: 'Y', isdate: 'N' },
            { table: 'Design', display: 'Retail Rate', name: 'retail', isnumber: 'Y', isdate: 'N' },
            { table: 'Design', display: 'Franchaise', name: 'franchaise', isnumber: 'Y', isdate: 'N' },

            { table: 'Location', display: 'Code', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'Company', name: 'company', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'Address', name: 'address', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'City', name: 'city', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'State', name: 'state', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'Warehouse', name: 'warehouse', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'Auto Settlement', name: 'autosettlement', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'Margin', name: 'margin', isnumber: 'Y', isdate: 'N' },
            { table: 'Location', display: 'GST No', name: 'gst', isnumber: 'N', isdate: 'N' },
            { table: 'Location', display: 'Contact No', name: 'contactno', isnumber: 'N', isdate: 'N' },

            { table: 'Item', display: 'PRODUCT', name: 'product', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'PRODUCT TYPE', name: 'producttype', isnumber: 'Y', isdate: 'N' },
            { table: 'Item', display: 'PRODUCT GROUP', name: 'productgroup', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'SUPPORT PO', name: 'supportpo', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'SUPPLIER', name: 'supplier', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'BRAND', name: 'brand', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'STYLE', name: 'style', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'TYPE', name: 'type', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'COLOUR', name: 'colour', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'SIZE', name: 'size', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'MATERIAL', name: 'material', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'PATTERN', name: 'pattern', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'CODE', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'GLN', name: 'gln', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'SHOWLIST', name: 'showlist', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'FIT', name: 'fit', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'SLEEVE', name: 'sleeve', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'PRINTING', name: 'printing', isnumber: 'N', isdate: 'N' },
            { table: 'Item', display: 'SALERATE', name: 'salerate', isnumber: 'Y', isdate: 'N' },
            { table: 'Item', display: 'PURCHASE RATE', name: 'purchaserate', isnumber: 'Y', isdate: 'N' },
            { table: 'Item', display: 'MRP', name: 'mrprate', isnumber: 'Y', isdate: 'N' },
            { table: 'Item', display: 'TAX PERCENTAGE', name: 'taxpercentage', isnumber: 'Y', isdate: 'N' },

            { table: 'Transport', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'Contact Person', name: 'contactperson', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'Contact No', name: 'contactno', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'Email ID', name: 'emailid', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'Address', name: 'address', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'City', name: 'cityid', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'State', name: 'stateid', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'Pincode', name: 'pincode', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'PAN', name: 'pan', isnumber: 'N', isdate: 'N' },
            { table: 'Transport', display: 'GST Number', name: 'gst', isnumber: 'N', isdate: 'N' },

            { table: 'Customer', display: 'Customer Code', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Customer Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Customer Billing Name', name: 'billingname', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Mobile Number', name: 'mobileno', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Category', name: 'categoryid', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Gender', name: 'gender', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Religion', name: 'religionid', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Email ID', name: 'emailid', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Date Of Birth', name: 'dateofbirth', isnumber: 'N', isdate: 'Y' },
            { table: 'Customer', display: 'Married', name: 'married', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Marrige Date', name: 'marriagedate', isnumber: 'N', isdate: 'Y' },
            { table: 'Customer', display: 'Loyelty Card Number', name: 'loyeltyno', isnumber: 'Y', isdate: 'N' },
            { table: 'Customer', display: 'Loyelty Point', name: 'loyelty', isnumber: 'Y', isdate: 'N' },
            { table: 'Customer', display: 'Margin', name: 'margin', isnumber: 'Y', isdate: 'N' },
            { table: 'Customer', display: 'GST Number', name: 'gstno', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Area', name: 'area', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Support Credit', name: 'supportcredit', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Credit Limit', name: 'creditlimit', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Address', name: 'address', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Delivery Address', name: 'deliveryaddress', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'City', name: 'cityid', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'PIN Code', name: 'pincode', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'State', name: 'stateid', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'Country', name: 'countryid', isnumber: 'N', isdate: 'N' },
            { table: 'Customer', display: 'LocationCode', name: 'locationcode', isnumber: 'N', isdate: 'N' },

            { table: 'Employee', display: 'LocationCode', name: 'locationcode', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'CompanyCode', name: 'companycode', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Code', name: 'employeecode', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Gender', name: 'gender', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Date of Birth', name: 'dateofbirth', isnumber: 'N', isdate: 'Y' },
            { table: 'Employee', display: 'Contact Number', name: 'contactno', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Address', name: 'address', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Date of Joining', name: 'dateofjoining', isnumber: 'N', isdate: 'Y' },
            { table: 'Employee', display: 'Aadhar No', name: 'aadharno', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'PAN No', name: 'pancardno', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'PF No', name: 'pfno', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'PF UAN No', name: 'pfuan', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'ESI No', name: 'esino', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'FLOOR', name: 'floor', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Department', name: 'department', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Designation', name: 'designation', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Section', name: 'section', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Resigned', name: 'hasleft', isnumber: 'N', isdate: 'N' },
            { table: 'Employee', display: 'Date of Relieving', name: 'relievingdate', isnumber: 'N', isdate: 'Y' },

            { table: 'Stock', display: 'LocationCode', name: 'LocationCode', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'CompanyCode', name: 'CompanyCode', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Barcode', name: 'BARCODE', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Product', name: 'PRODUCTNAME', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Brand', name: 'Brand', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Type', name: 'Type', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Style', name: 'Style', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Pattern', name: 'Pattern', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Colour', name: 'Colour', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Material', name: 'Material', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Section', name: 'Section', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Size', name: 'Size', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Fit', name: 'Fit', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Sleeve', name: 'Sleeve', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'PriceTag', name: 'PriceTag', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Design No', name: 'DESIGNNO', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'UAN No', name: 'GLN', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Item Code', name: 'ITBUSCODE', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Supplier', name: 'SUPPLIER', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Source Supplier', name: 'SOURCESUPPLIER', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Supplier City', name: 'SUPCITY', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Agent Name', name: 'AGENT', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Invoice No', name: 'INVNO', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Invoice Date', name: 'INVDATE', isnumber: 'N', isdate: 'Y' },
            { table: 'Stock', display: 'Purchase Entry Date', name: 'PURENTRYDATE', isnumber: 'N', isdate: 'Y' },
            { table: 'Stock', display: 'Purchase Price - (Inclusive Tax)', name: 'COST', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Purchase Discount Amount', name: 'PURCHASEDISCOUNT', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Purchase Tax Amount (Single Qty)', name: 'PurchaseTax1Amt', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Purchase Tax Percentage', name: 'PurchaseTaxPercentage', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Sale Rate', name: 'SALERATE', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'WS/Dealer Rate', name: 'WSRATE', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'MRP', name: 'MRP', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Sale Discount Amount', name: 'SALEDISCOUNT', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Sale Tax Percentage', name: 'taxpercentage', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Purchase Qty', name: 'PURQTY', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Sale Qty', name: 'SALQTY', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'Return Qty', name: 'RETQTY', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'BarcodeType', name: 'BarcodeType', isnumber: 'N', isdate: 'N' },
            { table: 'Stock', display: 'Stock Qty', name: 'CLOSINGQTY', isnumber: 'Y', isdate: 'N' },
            { table: 'Stock', display: 'HSN Code', name: 'HSNCODE', isnumber: 'N', isdate: 'N' },

            { table: 'UANStock', display: 'UAN', name: 'uan', isnumber: 'N', isdate: 'N' },
            { table: 'UANStock', display: 'LocationCode', name: 'LocationCode', isnumber: 'N', isdate: 'N' },
            { table: 'UANStock', display: 'CompanyCode', name: 'CompanyCode', isnumber: 'N', isdate: 'N' },
            { table: 'UANStock', display: 'Stock Qty', name: 'CLOSINGQTY', isnumber: 'Y', isdate: 'N' },
            { table: 'UANStock', display: 'Cost', name: 'cost', isnumber: 'Y', isdate: 'N' },
            
            { table: 'Sales', display: 'LocationCode', name: 'LocationCode', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'CompanyCode', name: 'CompanyCode', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Barcode', name: 'BARCODE', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Product', name: 'PRODUCTNAME', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Section', name: 'Section', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Brand', name: 'Brand', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Type', name: 'Type', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Style', name: 'Style', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Pattern', name: 'Pattern', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Colour', name: 'Colour', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Material', name: 'Material', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Size', name: 'Size', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Design No', name: 'DESIGNNO', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Supplier', name: 'SUPPLIER', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Supplier City', name: 'SUPCITY', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Agent Name', name: 'AGENT', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Invoice No', name: 'INVNO', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Invoice Date', name: 'INVDATE', isnumber: 'N', isdate: 'Y' },
            { table: 'Sales', display: 'Purchase Entry Date', name: 'PURENTRYDATE', isnumber: 'N', isdate: 'Y' },
            { table: 'Sales', display: 'Purchase Price', name: 'COST', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Purchase Discount Amount', name: 'PURCHASEDISCOUNT', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Purchase Tax Amount', name: 'PurchaseTax1Amt', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Purchase Tax Percentage', name: 'PurchaseTaxPercentage', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Bill Date', name: 'billdate', isnumber: 'N', isdate: 'Y' },
            { table: 'Sales', display: 'Bill Identifier', name: 'billid', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Bill No', name: 'billno', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'Sale Qty', name: 'SALQTY', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Sale Rate', name: 'SALERATE', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Sale Discount Amount', name: 'SALEDISCOUNT', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Sale Tax Percentage', name: 'taxpercentage', isnumber: 'Y', isdate: 'N' },
            { table: 'Sales', display: 'Salesmen Code', name: 'salesmancode', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'BarcodeType', name: 'BarcodeType', isnumber: 'N', isdate: 'N' },
            { table: 'Sales', display: 'HSN Code', name: 'HSNCODE', isnumber: 'N', isdate: 'N' },

            { table: 'CUSTOMERBALANCE', display: 'Location', name: 'location', isnumber: 'N', isdate: 'N' },
            { table: 'CUSTOMERBALANCE', display: 'Company', name: 'company', isnumber: 'N', isdate: 'N' },
            { table: 'CUSTOMERBALANCE', display: 'Customer Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'CUSTOMERBALANCE', display: 'Customer Mobile No', name: 'mobile', isnumber: 'N', isdate: 'N' },
            { table: 'CUSTOMERBALANCE', display: 'Bill NO', name: 'billno', isnumber: 'N', isdate: 'N' },
            { table: 'CUSTOMERBALANCE', display: 'Bill Date', name: 'billdate', isnumber: 'N', isdate: 'Y' },
            { table: 'CUSTOMERBALANCE', display: 'Balance Amount', name: 'balance', isnumber: 'Y', isdate: 'N' },

            { table: 'DEBITNOTE', display: 'CompanyCode', name: 'company', isnumber: 'N', isdate: 'N' },
            { table: 'DEBITNOTE', display: 'Supplier Name', name: 'name', isnumber: 'N', isdate: 'N' },
            { table: 'DEBITNOTE', display: 'Supplier City', name: 'city', isnumber: 'N', isdate: 'N' },
            { table: 'DEBITNOTE', display: 'Invoice No', name: 'invno', isnumber: 'N', isdate: 'N' },
            { table: 'DEBITNOTE', display: 'Return No', name: 'returnno', isnumber: 'N', isdate: 'N' },
            { table: 'DEBITNOTE', display: 'Invoice Date', name: 'invdate', isnumber: 'N', isdate: 'Y' },
            { table: 'DEBITNOTE', display: 'Return Date', name: 'returndate', isnumber: 'N', isdate: 'Y' },
            { table: 'DEBITNOTE', display: 'Balance Amount', name: 'balance', isnumber: 'Y', isdate: 'N' },

            { table: 'SUPPLIERBALANCE', display: 'CompanyCode', name: 'company', isnumber: 'N', isdate: 'N' },
            { table: 'SUPPLIERBALANCE', display: 'Supplier Name', name: 'suppliername', isnumber: 'N', isdate: 'N' },
            { table: 'SUPPLIERBALANCE', display: 'Invoice No', name: 'invno', isnumber: 'N', isdate: 'N' },
            { table: 'SUPPLIERBALANCE', display: 'Invoice Date', name: 'invdate', isnumber: 'N', isdate: 'Y' },
            { table: 'SUPPLIERBALANCE', display: 'Entry Date', name: 'entrydate', isnumber: 'N', isdate: 'Y' },
            { table: 'SUPPLIERBALANCE', display: 'Due Date', name: 'duedate', isnumber: 'N', isdate: 'Y' },
            { table: 'SUPPLIERBALANCE', display: 'Balance Amount', name: 'balance', isnumber: 'Y', isdate: 'N' },


            { table: 'Purchase', display: 'Bill No', name: 'Bill No', isnumber: 'N', isdate: 'N' },
            { table: 'Purchase', display: 'Bill Date', name: 'Bill Date', isnumber: 'N', isdate: 'Y' },
            { table: 'Purchase', display: 'Supplier Name', name: 'Supplier Name', isnumber: 'N', isdate: 'N' },
            { table: 'Purchase', display: 'Supplier GST', name: 'Supplier GST', isnumber: 'N', isdate: 'N' },
            { table: 'Purchase', display: 'LRNO', name: 'LRNO', isnumber: 'N', isdate: 'N' },
            { table: 'Purchase', display: 'Qty', name: 'Qty', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'MRP', name: 'MRP', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Cost Price', name: 'Cost Price', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Tax Percentage', name: 'Tax Percentage', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Tax Peritem', name: 'Tax Peritem', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Taxable Amount', name: 'Taxable Amount', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Total Tax', name: 'Total Tax', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Total Amount', name: 'Total Amount', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Price (Before Discount)', name: 'Price (Before Discount)', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Sales Discount %', name: 'Sales Discount %', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'Final Price', name: 'Final Price', isnumber: 'Y', isdate: 'N' },
            { table: 'Purchase', display: 'UAN', name: 'UAN', isnumber: 'N', isdate: 'N' },
            { table: 'Purchase', display: 'HSN', name: 'HSN', isnumber: 'N', isdate: 'N' },
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
            { table: 'Purchase', display: 'Full Name', name: 'Full Name', isnumber: 'N', isdate: 'N' },

            { table: 'Attribute', display: 'Barcode', name: 'Barcode', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Product', name: 'Product', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Brand', name: 'Brand', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Type', name: 'Type', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Style', name: 'Style', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Pattern', name: 'Pattern', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Colour', name: 'Colour', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Material', name: 'Material', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Size', name: 'Size', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Fit', name: 'Fit', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'Sleeve', name: 'Sleeve', isnumber: 'N', isdate: 'N' },
            { table: 'Attribute', display: 'PriceTag', name: 'PriceTag', isnumber: 'N', isdate: 'N' },

            { table: 'LREntry', display: 'Company', name: 'Company', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Section', name: 'Section', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Supplier', name: 'Supplier', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Entry No', name: 'EntryNo', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Entry Date', name: 'EntryDate', isnumber: 'N', isdate: 'Y' },
            { table: 'LREntry', display: 'Invoice No', name: 'InvoiceNo', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Invoice Date', name: 'InvoiceDate', isnumber: 'N', isdate: 'Y' },
            { table: 'LREntry', display: 'Transport', name: 'Transport', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Location', name: 'Location', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'From City', name: 'FromCity', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'To City', name: 'ToCity', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Remark', name: 'Remark', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'Purchase Manager', name: 'PurchaseManager', isnumber: 'N', isdate: 'N' },
            
            { table: 'LREntry', display: 'LR No', name: 'LRNo', isnumber: 'N', isdate: 'N' },
            { table: 'LREntry', display: 'LR Date', name: 'LRDate', isnumber: 'N', isdate: 'Y' },
            { table: 'LREntry', display: 'Actual Weight', name: 'ActualWeight', isnumber: 'Y', isdate: 'N' },
            { table: 'LREntry', display: 'Charged Weight', name: 'ChargedWeight', isnumber: 'Y', isdate: 'N' },
            { table: 'LREntry', display: 'No of Bundles', name: 'NoofBundles', isnumber: 'Y', isdate: 'N' },
            { table: 'LREntry', display: 'LR Amount', name: 'LRAmount', isnumber: 'Y', isdate: 'N' },
            { table: 'LREntry', display: 'Bill Value', name: 'BillValue', isnumber: 'Y', isdate: 'N' },
            

            { table: 'Multiple Price', display: 'barcode', name: 'barcode', isnumber: 'N', isdate: 'N' },
            { table: 'Multiple Price', display: 'price', name: 'price', isnumber: 'Y', isdate: 'N' },
            { table: 'Multiple Price', display: 'mrp', name: 'mrp', isnumber: 'Y', isdate: 'N' },
            { table: 'Multiple Price', display: 'location', name: 'location', isnumber: 'N', isdate: 'N' },

            { table: 'Vendor Agreement', display: 'barcode', name: 'barcode', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'item code', name: 'itemcode', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'product code', name: 'productcode', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'product', name: 'product', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'item name', name: 'itemname', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'vendor code', name: 'vendorcode', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'vendor name', name: 'vendorname', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'cost', name: 'cost', isnumber: 'Y', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'price', name: 'price', isnumber: 'Y', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'mrp', name: 'mrp', isnumber: 'Y', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'brand', name: 'brand', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'size', name: 'size', isnumber: 'N', isdate: 'N' },
            { table: 'Vendor Agreement', display: 'colour', name: 'colour', isnumber: 'N', isdate: 'N' },

            { table: 'Customer Credit Note Voucher', display: 'Code', name: 'code', isnumber: 'N', isdate: 'N' },
            { table: 'Customer Credit Note Voucher', display: 'Valid From', name: 'validfrom', isnumber: 'N', isdate: 'Y' },
            { table: 'Customer Credit Note Voucher', display: 'Valid To', name: 'validto', isnumber: 'N', isdate: 'Y' },
            { table: 'Customer Credit Note Voucher', display: 'Customer Name', name: 'customername', isnumber: 'N', isdate: 'N' },
            { table: 'Customer Credit Note Voucher', display: 'Customer MobileNo', name: 'customermobileno', isnumber: 'N', isdate: 'N' },
            { table: 'Customer Credit Note Voucher', display: 'Voucher Amount', name: 'voucheramount', isnumber: 'Y', isdate: 'N' }
        ];

        $scope.ExcelColumns = [];
        $scope.Tables = [];
        $scope.Columns = [];
        $scope.UploadedExcel = null;
        $scope.UploadedingGUID = null;
        $scope.TableName = null;
        $scope.Started = false;
        $scope.ProgressValue = 0;
        $scope.ExcelColumns = [];
        var TableMaps = [];
        $.each($scope.TableColumns, function (k,v) {

            if (typeof (TableMaps[v.table]) == 'undefined') {
                var table = { 'id': $scope.Tables.length+1, 'name': v.table, 'columns': [] };
                $scope.Tables.push(table);
                TableMaps[v.table] = table;
            }
            TableMaps[v.table].columns.push(v);
        });

        $scope.ExcelSheets = [];

        $scope.OnSheetChange = function () {

            var sheetindex = parseInt($scope.sheetid);
            if (isNaN(sheetindex)) return;
            $scope.Started = false;
            var value = $scope.UploadedExcel.sheets[sheetindex - 1];

            $scope.sheetname = value.name;
            $scope.ExcelColumns = [];
            $.each(value.columns, function (k, v) {
                $scope.ExcelColumns.push({ 'id': v, 'name': v });
            });

            $timeout(function () {
                var columns = localStorageService.get('MigrationData_' + $scope.TableName);
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
        }


        $scope.fileuploadedcomplete = function (value) {
            
            $scope.UploadedExcel = value;
            $scope.filename = null;
            $scope.ExcelSheets = value.sheets;            

            localStorageService.set('MigrationData', value);
        }

        $scope.TableChanged = function () {
            $scope.Started = false;
            var table = $scope.Tables.find(function (item) {
                return item.id == $scope.TableID;
            });

            
            if (typeof (table) != 'undefined' && table != null) {
                $scope.TableName = table.name;
                $scope.Columns = table.columns;
            }
        }

        $scope.SaveConfig = function () {
            localStorageService.set('MigrationData_' + $scope.TableName, $scope.Columns);
        }

        $scope.StartMigration = function () {
            localStorageService.set('MigrationData_' + $scope.TableName, $scope.Columns);
            var request = {
                'columns': $scope.Columns,
                'tablename': $scope.TableName,
                'sheetname': $scope.sheetname,
                'organizationid': $scope.OrganizationSelected && $scope.UserInfo.RootUser ? $scope.CurrentOrganization.id : 0,
                'uploadfileid': $scope.UploadedExcel.fileid
            };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "File/MigrateData",
                data: srequest
            }).success(function (result) {
                $scope.StatusID = result.identifier;
                $scope.UploadedingStatus = result;
                $scope.Started = true;
                $scope.ProgressValue = 0;
                $scope.migrationtimer = $timeout(function () {
                    $scope.CheckStatus();
                }, 2000);
            }).error(function (result) {
            });
        }

        $scope.CheckStatus = function () {
            restService.Get('File/MigrateStatus/' + $scope.UserInfo.OrganizationID +  '/' + $scope.StatusID).then(function (result) {
                $scope.UploadedingStatus = result;
                $scope.ProgressValue = result.percentage;
                if (!result.hascompleted) {
                    $scope.migrationtimer = $timeout(function () {
                        $scope.CheckStatus();
                    }, 2000);
                }
            });
        }

        $scope.migrationtimer = null;

        $scope.$on(
            "$destroy",
            function (event) {
                if ($scope.migrationtimer != null)
                    $timeout.cancel($scope.migrationtimer);
            }
        );

        $timeout(function () {
            var value = localStorageService.get('MigrationData');
            if (typeof (value) != 'undefined' && value != null) {
                $scope.fileuploadedcomplete(value);
            }
        }, 100);

    }]);