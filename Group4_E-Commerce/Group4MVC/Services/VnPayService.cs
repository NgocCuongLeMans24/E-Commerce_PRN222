﻿using DAL_Group4_E_Commerce.Models;
using GUI_Group4_ECommerce.Helpers;
using GUI_Group4_ECommerce.ViewModels;
using System;
using System.Diagnostics;

namespace GUI_Group4_ECommerce.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        public VnPayService(IConfiguration config) {
            _config = config;
        }
        //public string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model)
        //{
        //    var tick = DateTime.Now.Ticks.ToString();
        //    var vnpay = new VnPayLibrary();
        //    vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
        //    vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
        //    vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
        //    vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString()); 
        //    vnpay.AddRequestData("vnp_Createdate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
        //    vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);
        //    vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
        //    vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);
        //    vnpay.AddRequestData("vnp_OrderInfo", $"Payment for order: {model.OrderId}");
        //    vnpay.AddRequestData("vnp_OrderType", "other");
        //    vnpay.AddRequestData("vnp_ReturnUrl", _config["VnPay:PaymentBackReturnUrl"]);
        //    vnpay.AddRequestData("vnp_TxnRef", tick);

        //    var paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"], _config["VnPay:HashSecret"]);
        //    return paymentUrl;
        //}
        public string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
            //vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());
            var amountVnp = (long)Math.Round(model.Amount * 100);
            Console.WriteLine(">>>> model.Amount = " + model.Amount);
            Console.WriteLine(">>>> vnp_Amount = " + amountVnp);
            vnpay.AddRequestData("vnp_Amount", amountVnp.ToString(System.Globalization.CultureInfo.InvariantCulture));
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", model.OrderId.ToString());
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VnPay:PaymentBackReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", tick);
            return vnpay.CreateRequestUrl(
                _config["VnPay:BaseUrl"],
                _config["VnPay:HashSecret"]);   
        }


        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections) { 
                if(!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }
            var vnp_orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VnPay:HashSecret"]);
            if (!checkSignature) {
                return new VnPaymentResponseModel
                {
                    Success = false,
                    OrderId = vnp_OrderInfo,
                    VnPayResponseCode = vnp_ResponseCode
                };
            }
            return new VnPaymentResponseModel { 
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_OrderInfo,
                TransactionId = vnp_TransactionId.ToString(),
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }
}
