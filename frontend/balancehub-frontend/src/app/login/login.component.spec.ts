import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { LoginComponent } from './login.component';
import { AuthService } from '../services/auth.service';

// PrimeNG Modules for testing
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['login']);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        RouterTestingModule,
        HttpClientTestingModule,
        ReactiveFormsModule,
        CardModule,
        InputTextModule,
        PasswordModule,
        ButtonModule,
        CheckboxModule,
        MessageModule,
        ProgressSpinnerModule
      ],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Initialize the form
    component.ngOnInit();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form after ngOnInit', () => {
    component.ngOnInit();
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.get('email')).toBeDefined();
    expect(component.loginForm.get('password')).toBeDefined();
    expect(component.loginForm.get('role')).toBeDefined();
    expect(component.loginForm.get('rememberMe')).toBeDefined();
  });

  it('should initialize form with default values', () => {
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.get('email')).toBeDefined();
    expect(component.loginForm.get('password')).toBeDefined();
    expect(component.loginForm.get('role')).toBeDefined();
    expect(component.loginForm.get('rememberMe')).toBeDefined();

    expect(component.loginForm.get('role')?.value).toBe('Employee');
    expect(component.loginForm.get('rememberMe')?.value).toBe(false);
  });

  it('should validate email field as required', () => {
    const emailControl = component.loginForm.get('email');

    expect(emailControl?.valid).toBeFalsy();
    expect(emailControl?.hasError('required')).toBeTruthy();

    emailControl?.setValue('test@example.com');
    expect(emailControl?.valid).toBeTruthy();
  });

  it('should validate email field format', () => {
    const emailControl = component.loginForm.get('email');

    emailControl?.setValue('invalid-email');
    expect(emailControl?.hasError('email')).toBeTruthy();

    emailControl?.setValue('test@example.com');
    expect(emailControl?.hasError('email')).toBeFalsy();
  });

  it('should validate password field as required and minimum length', () => {
    const passwordControl = component.loginForm.get('password');

    expect(passwordControl?.valid).toBeFalsy();
    expect(passwordControl?.hasError('required')).toBeTruthy();

    passwordControl?.setValue('123');
    expect(passwordControl?.hasError('minlength')).toBeTruthy();

    passwordControl?.setValue('123456');
    expect(passwordControl?.valid).toBeTruthy();
  });

  it('should validate role field as required', () => {
    const roleControl = component.loginForm.get('role');

    expect(roleControl?.valid).toBeTruthy(); // Default value is set

    roleControl?.setValue('');
    expect(roleControl?.hasError('required')).toBeTruthy();
  });

  it('should call authService.login and navigate on successful login', () => {
    const mockResponse = {
      success: true,
      message: 'Login successful',
      token: 'mock-token',
      user: {
        id: '1',
        email: 'test@example.com',
        role: 'Employee'
      }
    };

    authServiceSpy.login.and.returnValue(of(mockResponse));

    component.loginForm.setValue({
      email: 'test@example.com',
      password: 'password123',
      role: 'Employee',
      rememberMe: false
    });

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalledWith('test@example.com', 'password123', 'Employee');
    expect(component.loading).toBe(false);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should handle login error', () => {
    authServiceSpy.login.and.returnValue(throwError(() => ({ success: false, message: 'Invalid credentials' })));

    component.loginForm.setValue({
      email: 'test@example.com',
      password: 'wrongpassword',
      role: 'Employee',
      rememberMe: false
    });

    component.onSubmit();

    expect(component.loading).toBe(false);
    expect(component.errorMessage).toBe('An error occurred during login');
    expect(routerSpy.navigate).not.toHaveBeenCalled();
  });

  it('should handle HTTP error', () => {
    const httpError = {
      error: {
        message: 'Server error'
      }
    };

    authServiceSpy.login.and.returnValue(throwError(() => httpError));

    component.loginForm.setValue({
      email: 'test@example.com',
      password: 'password123',
      role: 'Employee',
      rememberMe: false
    });

    component.onSubmit();

    expect(component.loading).toBe(false);
    expect(component.errorMessage).toBe('Server error');
  });

  it('should not submit when form is invalid', () => {
    component.loginForm.setValue({
      email: '',
      password: '',
      role: '',
      rememberMe: false
    });

    component.onSubmit();

    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('should mark all fields as touched when form is invalid and submitted', () => {
    spyOn(component.loginForm, 'markAsTouched');

    component.loginForm.setValue({
      email: '',
      password: '',
      role: '',
      rememberMe: false
    });

    component.onSubmit();

    expect(component.loginForm.markAsTouched).toHaveBeenCalled();
  });

  it('should provide correct form control getters', () => {
    expect(component.email).toBe(component.loginForm.get('email'));
    expect(component.password).toBe(component.loginForm.get('password'));
    expect(component.role).toBe(component.loginForm.get('role'));
  });

  it('should have correct roles array', () => {
    expect(component.roles).toEqual([
      { label: 'Employee', value: 'Employee' },
      { label: 'Manager', value: 'Manager' }
    ]);
  });

  it('should set loading state during login', () => {
    const mockResponse = {
      success: true,
      message: 'Login successful',
      token: 'mock-token',
      user: {
        id: '1',
        email: 'test@example.com',
        role: 'Employee'
      }
    };

    authServiceSpy.login.and.returnValue(of(mockResponse));

    component.loginForm.setValue({
      email: 'test@example.com',
      password: 'password123',
      role: 'Employee',
      rememberMe: false
    });

    component.onSubmit();

    expect(component.loading).toBe(false); // Should be false after successful login
  });
});
