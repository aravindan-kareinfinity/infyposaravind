
var app = angular.module('InfinityLab');

app.controller('DebitPaymentController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {

    $scope.ModeOfPayment = [{ 'id': 0, 'name': 'OneTime' }, { 'id': 1, 'name': 'EMI' }];

   
    $scope.ShowNew = false;

    $scope.ShowEMI = false;
    $scope.ShowOneTime = false;
    $scope.SearchResult = [];

    $scope.OneTime = function () {
        if ($scope.DebitPayment.paidbyemployeeid == 0) {
            $scope.ShowOneTime = true;
            $scope.ShowEMI = false;
        }
        else{
            $scope.ShowEMI = true;
            $scope.ShowOneTime = false;
        }
    }

    $scope.AddNew = function () {
        $scope.Academic = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ShowNew = true;
    }

  
    $scope.Entity = {
        "id": 0,
        "curstomerid": 0,
        "employeeid": 0,
        "paymenton": "",
        "paidamount": 0.0,
        "paymentinfo": null,
        "paymentmode": 0,
        "isactive": true,
        "createdby": 0,
        "modifiedby": 0,
        "version": 0,
        "createdon": "0001-01-01T00:00:00",
        "modifiedon": "0001-01-01T00:00:00",
        "billinformation": null,
        "emiinformation": null,
        "emiapprovedby": 0,
        "notes": null,
        "code": null
    };


    $scope.DebitPayment = (JSON.parse(JSON.stringify($scope.Entity)));

    $scope.SwitchPanel = function () {
        $scope.ShowNew = !$scope.ShowNew;
    }

    $scope.Save = function () {

        request = {
            "Item": $scope.Academic,
            "securitytoken": "String content"
        }

        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "PaymentsService/Save",
            data: srequest
        }).success(function (result) {

            if (result != null && result.id > 0) {
                $scope.ErrorMessage = "Successfully saved";
                $scope.ErrorStyle = "success";
                $scope.DebitPayment = (JSON.parse(JSON.stringify($scope.Entity)));
                $scope.Search();
            }
            else {
                $scope.ErrorMessage = result.ErrorMessage;
                $scope.ErrorStyle = "danger";
            }

        }).error(function (result) {
        });

    }

    $scope.SearchCustomer = function () {
        if ($scope.CurrentBill.phoneno == null || $scope.CurrentBill.phoneno == "") {
            $scope.CurrentBill.customername = "";
            $scope.setFocusBarcode();
            return;
        }
        restService.Get('CustomerService/PhoneNumber?id=' + $scope.CurrentBill.phoneno, true).then(function (results) {
            $scope.CurrentBill.customername = results.name;
            $scope.CurrentBill.customerid = results.id;
            $scope.setFocusBarcode();
        });
    }
    $timeout(function () {
        $scope.setFocusfirst();
    }, 10);

}]);
