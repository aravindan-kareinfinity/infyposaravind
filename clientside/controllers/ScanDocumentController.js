
var app = angular.module('InfinityLab');

app.controller('ScanDocumentController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    function (UserDetails, $scope, $http, $location, $rootScope, $window) {


        $scope.SearchRef = restService.SearchReference;

        $scope.Entity = {
            "id": 0,
            "cashierid": 0,
            "paidbyemployeeid": "",
            "receivedbyemployeid": "",
            "counterid": "",
            "paidon": "",
            "returnedon": "",
            "counterid": "",
            "locationid": "",
            "openingamount": "",
            "amount": "",
            "openingdinomination": [{ dinomination: 2000, qty: 0, amount: 0 }, { dinomination: 1000, qty: 0, amount: 0 }, { dinomination: 500, qty: 0, amount: 0 }, { dinomination: 100, qty: 0, amount: 0 }, { dinomination: 50, qty: 0, amount: 0 },
            { dinomination: 20, qty: 0, amount: 0 }, { dinomination: 10, qty: 0, amount: 0 }, { dinomination: 5, qty: 0, amount: 0 }, { dinomination: 2, qty: 0, amount: 0 }, { dinomination: 1, qty: 0, amount: 0 }],
            "counterid": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "status": 1
        };

        $scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
        });


$scope.ErrorStyle = "success";
$scope.ShowSearch = false;
$scope.SearchResult = [];

$scope.SwitchPanel = function () {
    $scope.ShowSearch = !$scope.ShowSearch;
    $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
}

$scope.ErrorMessage = "";



$scope.SearchCondition = {
    receivedbyemployeid:"",
    paidbyemployeeid: "",
    paidon:""
}

$scope.AmountChanged = function (item) {
    var oldValue = item.amount;
    item.amount = isNaN(item.dinomination * item.qty) ? 0 : item.dinomination * item.qty;
    $scope.OpeningBalance.openingamount = $scope.OpeningBalance.openingamount - oldValue + item.amount;
};

$scope.Save = function () {

    request = {
        "Item": $scope.OpeningBalance,
        "securitytoken": "String content"
    }

    var srequest = JSON.stringify(request);

    $http({
        method: 'POST',
        url: "CashService/Save",
        data: srequest
    }).success(function (result) {

        if (result != null && result.id > 0) {
            $scope.ErrorMessage = "Successfully saved";
            $scope.ErrorStyle = "success";
            $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        else {
            $scope.ErrorMessage = result.ErrorMessage;
            $scope.ErrorStyle = "danger";
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
            confirm: function () {

                request = {
                    "Item": item,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "CashService/Delete",
                    data: srequest
                }).success(function (result) {

                    if (result) {
                        $scope.ErrorMessage = "Successfully deleted";
                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                    }

                }).error(function (result) {

                });
            }
        });


    }
    else if (action == "Edit") {
        $scope.ShowSearch = false;
        $scope.OpeningBalance = item;
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
        url: "CashService/Search",
        data: srequest
    }).success(function (result) {

        $scope.SearchResult = result;
    }).error(function (result) {
    });

}
}]);