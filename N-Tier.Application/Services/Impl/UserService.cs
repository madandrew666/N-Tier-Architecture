﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using N_Tier.Application.Exceptions;
using N_Tier.Application.Helpers;
using N_Tier.Application.Models.User;
using N_Tier.Infrastructure.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace N_Tier.Application.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserService(IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Guid> CreateAsync(CreateUserModel createUserModel)
        {
            var user = _mapper.Map<ApplicationUser>(createUserModel);

            var result = await _userManager.CreateAsync(user, createUserModel.Password);

            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.FirstOrDefault().Description);
            }

            // TODO send email with confirmation token

            return Guid.Parse((await _userManager.FindByNameAsync(user.UserName)).Id);
        }

        public async Task<LoginResponseModel> LoginAsync(LoginUserModel loginUserModel)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginUserModel.Username);

            if (user == null)
                throw new NotFoundException("Username or password is incorrect");


            var signInResult = await _signInManager.PasswordSignInAsync(user, loginUserModel.Password, false, false);

            if (!signInResult.Succeeded)
                throw new BadRequestException("Username or password is incorrect");


            var token = JwtHelper.GenerateToken(user);

            return new LoginResponseModel()
            {
                Username = user.UserName,
                Email = user.Email,
                Token = token
            };
        }
    }
}